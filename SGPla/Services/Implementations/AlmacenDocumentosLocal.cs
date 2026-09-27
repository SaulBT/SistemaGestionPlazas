using System.Security.Cryptography;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class AlmacenDocumentosLocal : IAlmacenDocumentos
{
    public const long TamanoMaximo = 25 * 1024 * 1024;
    private const string MimePermitido = "application/pdf";
    private readonly string _raiz;
    private readonly string _directorioAvisos;
    private readonly string _directorioAspirantes;
    private readonly string _directorioActas;
    private readonly string _directorioCuarentena;

    public AlmacenDocumentosLocal(IConfiguration configuration, IHostEnvironment environment)
    {
        var configurado = configuration["Documentos:Directorio"];
        var raiz = string.IsNullOrWhiteSpace(configurado)
            ? Path.Combine(environment.ContentRootPath, "App_Data", "documentos")
            : Path.IsPathRooted(configurado)
                ? configurado
                : Path.Combine(environment.ContentRootPath, configurado);
        _raiz = Path.GetFullPath(raiz);
        _directorioAvisos = Path.Combine(_raiz, "avisos");
        _directorioAspirantes = Path.Combine(_raiz, "aspirantes");
        _directorioActas = Path.Combine(_raiz, "actas");
        _directorioCuarentena = Path.Combine(_raiz, ".cuarentena-eliminacion");
    }

    public async Task<DocumentoAlmacenado> GuardarOriginalAsync(
        Stream contenido, string nombreOriginal, string mime, long tamanoDeclarado,
        CancellationToken cancellationToken = default) =>
        await GuardarEnCategoriaAsync(contenido, nombreOriginal, mime, tamanoDeclarado,
            "avisos", _directorioAvisos, cancellationToken);

    public async Task<DocumentoAlmacenado> GuardarAspiranteAsync(
        Stream contenido, string nombreOriginal, string mime, long tamanoDeclarado,
        CancellationToken cancellationToken = default) =>
        await GuardarEnCategoriaAsync(contenido, nombreOriginal, mime, tamanoDeclarado,
            "aspirantes", _directorioAspirantes, cancellationToken);

    public async Task<DocumentoAlmacenado> GuardarActaAsync(
        Stream contenido, string nombreOriginal, string mime, long tamanoDeclarado,
        CancellationToken cancellationToken = default) =>
        await GuardarEnCategoriaAsync(contenido, nombreOriginal, mime, tamanoDeclarado,
            "actas", _directorioActas, cancellationToken);

    private static async Task<DocumentoAlmacenado> GuardarEnCategoriaAsync(
        Stream contenido, string nombreOriginal, string mime, long tamanoDeclarado,
        string categoria, string directorioCategoria, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(contenido);
        if (!contenido.CanRead) throw new ArgumentException("El documento no se puede leer.", nameof(contenido));
        if (!string.Equals(mime?.Trim(), MimePermitido, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("El documento debe ser un PDF.", nameof(mime));
        if (tamanoDeclarado is <= 0 or > TamanoMaximo)
            throw new ArgumentException("El documento debe tener entre 1 byte y 25 MB.", nameof(tamanoDeclarado));

        var nombre = NormalizarNombre(nombreOriginal);
        Directory.CreateDirectory(directorioCategoria);
        var id = Guid.NewGuid().ToString("N");
        var temporal = Path.Combine(directorioCategoria, $".{id}.tmp");
        var destino = Path.Combine(directorioCategoria, $"{id}.pdf");
        var clave = $"{categoria}/{id}.pdf";
        long escrito = 0;
        byte[] checksum;
        try
        {
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            await using (var salida = new FileStream(temporal, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                             81920, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                var buffer = new byte[81920];
                int leidos;
                while ((leidos = await contenido.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    escrito = checked(escrito + leidos);
                    if (escrito > TamanoMaximo || escrito > tamanoDeclarado)
                        throw new InvalidDataException("El contenido excede el tamaño permitido o declarado.");
                    hash.AppendData(buffer, 0, leidos);
                    await salida.WriteAsync(buffer.AsMemory(0, leidos), cancellationToken);
                }
                await salida.FlushAsync(cancellationToken);
                checksum = hash.GetHashAndReset();
            }

            if (escrito != tamanoDeclarado)
                throw new InvalidDataException("El tamaño del contenido no coincide con el declarado.");
            ValidarFirmaPdf(temporal);
            File.Move(temporal, destino);
            return new DocumentoAlmacenado(nombre, MimePermitido, escrito, checksum, clave);
        }
        catch
        {
            if (File.Exists(temporal)) File.Delete(temporal);
            throw;
        }
    }

    public Task<Stream?> AbrirLecturaAsync(string claveRelativa, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var ruta = ResolverClave(claveRelativa);
        Stream? stream = File.Exists(ruta)
            ? new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.Read, 81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan)
            : null;
        return Task.FromResult(stream);
    }

    public Task EliminarAsync(string claveRelativa, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var ruta = ResolverClave(claveRelativa);
        if (File.Exists(ruta)) File.Delete(ruta);
        return Task.CompletedTask;
    }

    public Task<DocumentoEnCuarentena?> PrepararEliminacionAsync(string claveRelativa,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var origen = ResolverClave(claveRelativa);
        if (!File.Exists(origen)) return Task.FromResult<DocumentoEnCuarentena?>(null);
        Directory.CreateDirectory(_directorioCuarentena);
        var identificador = Guid.NewGuid().ToString("N");
        var destino = RutaCuarentena(identificador);
        File.Move(origen, destino);
        return Task.FromResult<DocumentoEnCuarentena?>(new DocumentoEnCuarentena(claveRelativa, identificador));
    }

    public Task RestaurarEliminacionAsync(DocumentoEnCuarentena documento,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(documento);
        var original = ResolverClave(documento.ClaveRelativa);
        var cuarentena = RutaCuarentena(documento.Identificador);
        if (!File.Exists(cuarentena)) return Task.CompletedTask;
        if (File.Exists(original)) throw new IOException("No se puede restaurar el documento porque la clave original ya existe.");
        Directory.CreateDirectory(Path.GetDirectoryName(original)!);
        File.Move(cuarentena, original);
        return Task.CompletedTask;
    }

    public Task CompletarEliminacionAsync(DocumentoEnCuarentena documento,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(documento);
        var cuarentena = RutaCuarentena(documento.Identificador);
        if (File.Exists(cuarentena)) File.Delete(cuarentena);
        return Task.CompletedTask;
    }

    private string ResolverClave(string clave)
    {
        var (categoria, longitudPrefijo) = clave?.StartsWith("avisos/", StringComparison.Ordinal) == true
            ? ("avisos", 7)
            : clave?.StartsWith("aspirantes/", StringComparison.Ordinal) == true
                ? ("aspirantes", 11)
                : clave?.StartsWith("actas/", StringComparison.Ordinal) == true
                    ? ("actas", 6)
                    : (string.Empty, 0);
        if (string.IsNullOrWhiteSpace(clave) || categoria.Length == 0 || clave.Length != longitudPrefijo + 36
            || !clave.EndsWith(".pdf", StringComparison.Ordinal)
            || clave.AsSpan(longitudPrefijo, 32).ContainsAnyExcept("0123456789abcdef"))
            throw new ArgumentException("La clave de almacenamiento no es válida.", nameof(clave));

        var ruta = Path.GetFullPath(Path.Combine(_raiz, clave.Replace('/', Path.DirectorySeparatorChar)));
        var directorioCategoria = categoria switch
        {
            "avisos" => _directorioAvisos,
            "aspirantes" => _directorioAspirantes,
            _ => _directorioActas
        };
        var prefijo = directorioCategoria + Path.DirectorySeparatorChar;
        if (!ruta.StartsWith(prefijo, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new ArgumentException("La clave de almacenamiento no es válida.", nameof(clave));
        return ruta;
    }

    private string RutaCuarentena(string identificador)
    {
        if (identificador.Length != 32 || identificador.AsSpan().ContainsAnyExcept("0123456789abcdef"))
            throw new ArgumentException("El identificador de cuarentena no es válido.", nameof(identificador));
        var ruta = Path.GetFullPath(Path.Combine(_directorioCuarentena, identificador + ".pending"));
        var prefijo = _directorioCuarentena + Path.DirectorySeparatorChar;
        if (!ruta.StartsWith(prefijo, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new ArgumentException("El identificador de cuarentena no es válido.", nameof(identificador));
        return ruta;
    }

    private static string NormalizarNombre(string nombreOriginal)
    {
        var nombre = Path.GetFileName((nombreOriginal ?? string.Empty).Replace('\\', '/'));
        nombre = new string(nombre.Where(c => !char.IsControl(c)).ToArray()).Trim();
        if (nombre.Length is 0 or > 260 || !nombre.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("El nombre del documento debe ser un PDF de hasta 260 caracteres.", nameof(nombreOriginal));
        return nombre;
    }

    private static void ValidarFirmaPdf(string ruta)
    {
        Span<byte> firma = stackalloc byte[5];
        using var stream = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Read(firma) != firma.Length || !firma.SequenceEqual("%PDF-"u8))
            throw new InvalidDataException("El contenido no tiene una firma PDF válida.");
    }
}
