namespace SGPla.Services.Interfaces;

public interface IAlmacenDocumentos
{
    Task<DocumentoAlmacenado> GuardarOriginalAsync(
        Stream contenido, string nombreOriginal, string mime, long tamanoDeclarado,
        CancellationToken cancellationToken = default);

    Task<Stream?> AbrirLecturaAsync(string claveRelativa, CancellationToken cancellationToken = default);

    Task EliminarAsync(string claveRelativa, CancellationToken cancellationToken = default);
}

public sealed record DocumentoAlmacenado(
    string Nombre,
    string Mime,
    long Tamano,
    byte[] ChecksumSha256,
    string ClaveRelativa);
