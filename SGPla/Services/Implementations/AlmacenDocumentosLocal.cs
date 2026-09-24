using System.Security.Cryptography;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class AlmacenDocumentosLocal : IAlmacenDocumentos
{
    public const long TamanoMaximo = 25 * 1024 * 1024;
    private const string MimePermitido = "application/pdf";
    private readonly string _raiz;
    private readonly string _directorioAvisos;

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
    }

    public async Task<DocumentoAlmacenado> GuardarOriginalAsync(
        Stream contenido, string nombreOriginal, string mime, long tamanoDeclarado,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contenido);
        if (!contenido.CanRead) throw new ArgumentException("El documento no se puede leer.", nameof(contenido));
        if (!string.Equals(mime?.Trim(), MimePermitido, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("El documento debe ser un PDF.", nameof(mime));
        if (tamanoDeclarado is <= 0 or > TamanoMaximo)
            throw new ArgumentException("El documento debe tener entre 1 byte y 25 MB.", nameof(tamanoDeclarado));

        var nombre = NormalizarNombre(nombreOriginal);
        Directory.CreateDirectory(_directorioAvisos);
        var id = Guid.NewGuid().ToString("N");
        var temporal = Path.Combine(_directorioAvisos, $".{id}.tmp");
        var destino = Path.Combine(_directorioAvisos, $"{id}.pdf");
        var clave = $"avisos/{id}.pdf";
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

    private string ResolverClave(string clave)
    {
        if (string.IsNullOrWhiteSpace(clave) || clave.Length != 43
            || !clave.StartsWith("avisos/", StringComparison.Ordinal)
            || !clave.EndsWith(".pdf", StringComparison.Ordinal)
            || clave.AsSpan(7, 32).ContainsAnyExcept("0123456789abcdef"))
            throw new ArgumentException("La clave de almacenamiento no es válida.", nameof(clave));

        var ruta = Path.GetFullPath(Path.Combine(_raiz, clave.Replace('/', Path.DirectorySeparatorChar)));
        var prefijo = _directorioAvisos + Path.DirectorySeparatorChar;
        if (!ruta.StartsWith(prefijo, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new ArgumentException("La clave de almacenamiento no es válida.", nameof(clave));
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
