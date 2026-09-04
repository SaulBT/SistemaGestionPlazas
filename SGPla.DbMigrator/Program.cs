using DbUp;
using DbUp.Engine;

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
                    "development_seed.sql",
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

sealed class StripBomPreprocessor : IScriptPreprocessor
{
    public string Process(string contents) => contents.TrimStart('\uFEFF');
}
