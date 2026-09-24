using SGPla.Models.DTOs.Avisos;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class DocumentoAvisoMvcService : IDocumentoAvisoMvcService
{
    private readonly IAlmacenDocumentos _almacen;
    private readonly IDocumentoAvisoMvcRepository _repository;

    public DocumentoAvisoMvcService(IAlmacenDocumentos almacen, IDocumentoAvisoMvcRepository repository)
    {
        _almacen = almacen;
        _repository = repository;
    }

    public async Task<int> GuardarOriginalAsync(int avisoId, int entidadAcademicaId, int usuarioId,
        Stream contenido, string nombre, string mime, long tamanoDeclarado,
        CancellationToken cancellationToken = default)
    {
        if (avisoId < 1 || entidadAcademicaId < 1 || usuarioId < 1)
            throw new ArgumentException("El Aviso, la Entidad Académica y la cuenta deben ser válidos.");

        await _repository.ValidarCargaOriginalAsync(avisoId, entidadAcademicaId, usuarioId, cancellationToken);
        var almacenado = await _almacen.GuardarOriginalAsync(contenido, nombre, mime, tamanoDeclarado, cancellationToken);
        try
        {
            return await _repository.GuardarOriginalAsync(new DocumentoAvisoOriginalMvc(
                avisoId, entidadAcademicaId, usuarioId, almacenado.Nombre, almacenado.Mime,
                almacenado.Tamano, almacenado.ChecksumSha256, almacenado.ClaveRelativa), cancellationToken);
        }
        catch (Exception errorPersistencia)
        {
            try
            {
                await _almacen.EliminarAsync(almacenado.ClaveRelativa, CancellationToken.None);
            }
            catch (Exception errorCompensacion)
            {
                throw new AggregateException(
                    "Falló el guardado de metadatos y también la compensación del archivo; requiere conciliación operativa.",
                    errorPersistencia, errorCompensacion);
            }
            throw;
        }
    }

    public async Task<DescargaDocumentoAvisoMvc?> AbrirParaDescargaAsync(int avisoId, int documentoId,
        int usuarioId, CancellationToken cancellationToken = default)
    {
        if (avisoId < 1 || documentoId < 1 || usuarioId < 1)
            throw new ArgumentException("El Aviso, documento y cuenta deben ser válidos.");
        var metadata = await _repository.ObtenerParaDescargaAsync(avisoId, documentoId, usuarioId, cancellationToken);
        if (metadata is null) return null;
        var contenido = await _almacen.AbrirLecturaAsync(metadata.ClaveAlmacenamiento, cancellationToken);
        if (contenido is null)
            throw new FileNotFoundException("El archivo asociado al documento no está disponible.");
        return new DescargaDocumentoAvisoMvc(contenido, metadata.Nombre, metadata.Mime);
    }
}
