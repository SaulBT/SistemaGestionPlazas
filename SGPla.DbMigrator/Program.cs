using DbUp;
using DbUp.Engine;
using Konscious.Security.Cryptography;
using Microsoft.Data.SqlClient;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

const int maxAttempts = 10;
string connectionString;
try
{
    connectionString = GetConnectionString(args);
}
catch (InvalidOperationException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 2;
}

if (args.Contains("--bootstrap-superusuario", StringComparer.OrdinalIgnoreCase))
    return await BootstrapSuperusuarioAsync(connectionString);
if (args.Contains("--preflight", StringComparer.OrdinalIgnoreCase))
    return await PreflightAsync(connectionString);

var databasePath = Path.Combine(AppContext.BaseDirectory, "database");
var baselinePath = Path.Combine(databasePath, "baseline_schema.sql");
var migrationsPath = Path.Combine(databasePath, "migrations");
var seedPath = Path.Combine(databasePath, "seed", "development.sql");

if (!File.Exists(baselinePath))
{
    Console.Error.WriteLine($"No se encontró el baseline de la base de datos: {baselinePath}");
    return 2;
}

for (var attempt = 1; attempt <= maxAttempts; attempt++)
{
    try
    {
        EnsureDatabase.For.SqlDatabase(connectionString);

        var builder = DeployChanges.To
            .SqlDatabase(connectionString)
            // The file name gives the baseline the same deterministic ordering as
            // every future migration, while keeping the existing source file intact.
            .WithScripts(new SqlScript(
                "0001_baseline_schema.sql",
                // The legacy script is UTF-8 with BOM. SQL Server treats that BOM
                // as an unexpected token when DbUp executes the first batch. The
                // database bootstrap is also removed because EnsureDatabase has
                // already selected the database from the supplied connection.
                PrepareBaseline(File.ReadAllText(baselinePath))))
            .WithPreprocessor(new StripBomPreprocessor())
            .LogToConsole()
            .WithTransaction();

        if (Directory.Exists(migrationsPath))
        {
            builder = builder.WithScriptsFromFileSystem(migrationsPath);
        }

        var result = builder.Build().PerformUpgrade();

        if (!result.Successful)
        {
            Console.Error.WriteLine(result.Error);
            return 1;
        }

        if (ShouldApplyDevelopmentSeed())
        {
            if (!File.Exists(seedPath))
            {
                Console.Error.WriteLine($"No se encontró el seed de desarrollo: {seedPath}");
                return 2;
            }

            var seedResult = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScripts(new SqlScript(
                    // Nueva versión idempotente del seed para ajustar secuencias después
                    // de insertar los catálogos de desarrollo.
                    "development_seed_0027.sql",
                    File.ReadAllText(seedPath).TrimStart('\uFEFF')))
                .WithPreprocessor(new StripBomPreprocessor())
                .LogToConsole()
                .WithTransaction()
                .Build()
                .PerformUpgrade();

            if (!seedResult.Successful)
            {
                Console.Error.WriteLine(seedResult.Error);
                return 1;
            }
        }

        Console.WriteLine("Migraciones aplicadas correctamente.");
        return 0;
    }
    catch (Exception exception) when (attempt < maxAttempts)
    {
        Console.WriteLine(
            $"La base de datos aún no está disponible (intento {attempt}/{maxAttempts}). " +
            $"Reintentando en 3 segundos: {exception.Message}");
        await Task.Delay(TimeSpan.FromSeconds(3));
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"No fue posible ejecutar las migraciones: {exception}");
        return 1;
    }
}

return 1;

static string PrepareBaseline(string sql)
{
    const string databaseBootstrap =
        @"IF DB_ID(N'GestionDePlazasBD') IS NULL
BEGIN
    CREATE DATABASE [GestionDePlazasBD];
END
GO

USE [GestionDePlazasBD]
GO";

    var withoutBom = sql.TrimStart('\uFEFF').Replace("\r\n", "\n");
    return withoutBom
        .Replace(databaseBootstrap, string.Empty, StringComparison.OrdinalIgnoreCase)
        .Replace("USE [master]", string.Empty, StringComparison.OrdinalIgnoreCase);
}

static string GetConnectionString(string[] args)
{
    var argument = args.FirstOrDefault(value =>
        value.StartsWith("--connection=", StringComparison.OrdinalIgnoreCase));

    var connectionString = argument is not null
        ? argument["--connection=".Length..]
        : Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
          ?? Environment.GetEnvironmentVariable("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Defina ConnectionStrings__DefaultConnection o use --connection=<cadena>. ");
    }

    return connectionString;
}

static bool ShouldApplyDevelopmentSeed()
{
    return bool.TryParse(
        Environment.GetEnvironmentVariable("MIGRATOR_APPLY_DEVELOPMENT_SEED"),
        out var applySeed) && applySeed;
}

static async Task<int> BootstrapSuperusuarioAsync(string connectionString)
{
    if (Console.IsInputRedirected || Console.IsOutputRedirected)
    {
        Console.Error.WriteLine("El bootstrap requiere una consola interactiva para capturar la contraseña sin eco.");
        return 2;
    }

    var correo = Prompt("Correo del primer Superusuario: ").Trim().ToLowerInvariant();
    var nombre = Prompt("Nombre del primer Superusuario: ").Trim();
    if (!EsCorreoValido(correo) || nombre.Length is 0 or > 200)
    {
        Console.Error.WriteLine("Correo o nombre inválidos.");
        return 2;
    }

    try
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        var destination = new SqlConnectionStringBuilder(connectionString);
        await using (var destinationCommand = new SqlCommand(
                         "SELECT CONVERT(nvarchar(128), SERVERPROPERTY('ServerName'));", connection))
        {
            var serverName = Convert.ToString(await destinationCommand.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture);
            Console.WriteLine($"Destino: {serverName} / {destination.InitialCatalog}");
        }
        Console.Write($"Para continuar escriba exactamente el nombre de la base ({destination.InitialCatalog}): ");
        if (string.IsNullOrWhiteSpace(destination.InitialCatalog) ||
            !string.Equals(Console.ReadLine(), destination.InitialCatalog, StringComparison.Ordinal))
        {
            Console.Error.WriteLine("Destino no confirmado; no se creó ninguna cuenta.");
            return 2;
        }

        var password = ReadSecret("Contraseña temporal: ");
        var confirmation = ReadSecret("Confirme la contraseña temporal: ");
        if (!string.Equals(password, confirmation, StringComparison.Ordinal) || !EsContrasenaValida(password))
        {
            Console.Error.WriteLine("La contraseña no coincide o no cumple la política configurada.");
            return 2;
        }

        var phc = HashPassword(password);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        await using (var lockCommand = new SqlCommand(
                         "SELECT COUNT_BIG(*) FROM [usuarios].[usuario] WITH (UPDLOCK, HOLDLOCK) WHERE [rol_id] = 1;",
                         connection, transaction))
        {
            var count = Convert.ToInt64(await lockCommand.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture);
            if (count != 0)
            {
                await transaction.RollbackAsync();
                Console.Error.WriteLine("El bootstrap sólo se permite antes de crear cualquier cuenta Superusuario.");
                return 2;
            }
        }

        await using (var roleCommand = new SqlCommand(
                         "SELECT COUNT(*) FROM [usuarios].[rol] WHERE [id] = 1 AND [nombre] = N'Superusuario';",
                         connection, transaction))
        {
            if (Convert.ToInt32(await roleCommand.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture) != 1)
            {
                await transaction.RollbackAsync();
                Console.Error.WriteLine("El catálogo normalizado no contiene el rol Superusuario esperado.");
                return 2;
            }
        }

        int usuarioId;
        await using (var insertUser = new SqlCommand(
                         "INSERT INTO [usuarios].[usuario] ([correo], [nombre], [rol_id]) OUTPUT INSERTED.[id] VALUES (@correo, @nombre, 1);",
                         connection, transaction))
        {
            insertUser.Parameters.Add("@correo", System.Data.SqlDbType.VarChar, 254).Value = correo;
            insertUser.Parameters.Add("@nombre", System.Data.SqlDbType.NVarChar, 200).Value = nombre;
            usuarioId = Convert.ToInt32(await insertUser.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture);
        }

        await using (var insertCredential = new SqlCommand(
                         "INSERT INTO [usuarios].[credencial_superusuario] ([usuario_id], [contrasena], [fecha_actualizacion]) VALUES (@usuarioId, @phc, NULL);",
                         connection, transaction))
        {
            insertCredential.Parameters.Add("@usuarioId", System.Data.SqlDbType.Int).Value = usuarioId;
            insertCredential.Parameters.Add("@phc", System.Data.SqlDbType.VarChar, 500).Value = phc;
            await insertCredential.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
        Console.WriteLine("Bootstrap completado. La primera autenticación sólo permitirá cambiar la contraseña temporal.");
        return 0;
    }
    catch (SqlException)
    {
        Console.Error.WriteLine("No fue posible completar el bootstrap. No se muestran detalles de conexión ni credenciales.");
        return 1;
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"No fue posible completar el bootstrap: {exception.GetType().Name}.");
        return 1;
    }
}

static async Task<int> PreflightAsync(string connectionString)
{
    try
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using (var target = new SqlCommand(
                         "SELECT CONVERT(nvarchar(128), SERVERPROPERTY('ServerName')), DB_NAME();", connection))
        await using (var reader = await target.ExecuteReaderAsync())
        {
            await reader.ReadAsync();
            Console.WriteLine($"Destino confirmado: {reader.GetString(0)} / {reader.GetString(1)}");
        }

        await using var existeCommand = new SqlCommand(
            "SELECT CONVERT(bit, CASE WHEN OBJECT_ID(N'dbo.SchemaVersions', N'U') IS NULL THEN 0 ELSE 1 END);", connection);
        var existe = Convert.ToBoolean(await existeCommand.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture);
        var cantidad = 0;
        if (existe)
        {
            await using var journal = new SqlCommand(
                "SELECT ScriptName, Applied FROM dbo.SchemaVersions ORDER BY Applied, ScriptName;", connection);
            await using var rows = await journal.ExecuteReaderAsync();
            while (await rows.ReadAsync())
            {
                cantidad++;
                Console.WriteLine($"DbUp: {rows.GetString(0)} ({rows.GetDateTime(1):O})");
            }
        }
        Console.WriteLine($"Journal dbo.SchemaVersions: {(existe ? $"presente, {cantidad} entradas" : "ausente")}");
        if (existe)
        {
            await using var users = new SqlCommand(
                "SELECT COUNT_BIG(*) FROM [usuarios].[usuario] WHERE [rol_id] = 1;", connection);
            Console.WriteLine($"Cuentas Superusuario: {Convert.ToInt64(await users.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture)}");
        }
        return existe ? 0 : 1;
    }
    catch (SqlException exception)
    {
        Console.Error.WriteLine($"Preflight fallido (SqlException {exception.Number}, estado {exception.State}); no se muestran detalles de conexión.");
        return 1;
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"Preflight fallido ({exception.GetType().Name}); no se muestran detalles de conexión.");
        return 1;
    }
}

static string Prompt(string prompt)
{
    Console.Write(prompt);
    return Console.ReadLine() ?? string.Empty;
}

static string ReadSecret(string prompt)
{
    Console.Write(prompt);
    var characters = new List<char>(129);
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            return new string(characters.ToArray());
        }
        if (key.Key == ConsoleKey.Backspace)
        {
            if (characters.Count > 0) characters.RemoveAt(characters.Count - 1);
            continue;
        }
        if (!char.IsControl(key.KeyChar) && characters.Count < 129)
            characters.Add(key.KeyChar);
    }
}

static bool EsCorreoValido(string correo)
{
    if (correo.Length is < 3 or > 254 || correo.Any(char.IsWhiteSpace)) return false;
    try { return new MailAddress(correo).Address == correo; }
    catch (FormatException) { return false; }
}

static bool EsContrasenaValida(string password) =>
    password.Length is >= 8 and <= 128 && password.Any(char.IsUpper) && password.Any(char.IsLower) &&
    password.Any(char.IsDigit) && password.Any(c => !char.IsLetterOrDigit(c));

static string HashPassword(string password)
{
    var passwordBytes = new UTF8Encoding(false, true).GetBytes(password);
    var salt = RandomNumberGenerator.GetBytes(16);
    try
    {
        using var argon2 = new Argon2id(passwordBytes)
        {
            Salt = salt,
            MemorySize = 19_456,
            Iterations = 2,
            DegreeOfParallelism = 1
        };
        var derived = argon2.GetBytes(32);
        try
        {
            return $"$argon2id$v=19$m=19456,t=2,p=1${Convert.ToBase64String(salt).TrimEnd('=')}${Convert.ToBase64String(derived).TrimEnd('=')}";
        }
        finally { CryptographicOperations.ZeroMemory(derived); }
    }
    finally { CryptographicOperations.ZeroMemory(passwordBytes); }
}

sealed class StripBomPreprocessor : IScriptPreprocessor
{
    public string Process(string contents) => contents.TrimStart('\uFEFF');
}
