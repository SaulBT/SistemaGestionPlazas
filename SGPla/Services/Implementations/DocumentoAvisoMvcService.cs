using SGPla.Models.DTOs.Avisos;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using System.Security.Cryptography;

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
            await CompensarSoloSiNoHayReferenciaAsync(almacenado.ClaveRelativa, errorPersistencia);
            throw;
        }
    }

    public async Task<int> GuardarFirmadoAsync(int avisoId, int entidadAcademicaId, int usuarioId,
        Stream contenido, string nombre, string mime, long tamanoDeclarado,
        CancellationToken cancellationToken = default)
    {
        if (avisoId < 1 || entidadAcademicaId < 1 || usuarioId < 1)
            throw new ArgumentException("El Aviso, la Entidad Académica y la cuenta deben ser válidos.");

        await _repository.ValidarCargaFirmadoAsync(avisoId, entidadAcademicaId, usuarioId, cancellationToken);
        var almacenado = await _almacen.GuardarOriginalAsync(contenido, nombre, mime, tamanoDeclarado, cancellationToken);
        try
        {
            return await _repository.GuardarFirmadoAsync(new DocumentoAvisoOriginalMvc(
                avisoId, entidadAcademicaId, usuarioId, almacenado.Nombre, almacenado.Mime,
                almacenado.Tamano, almacenado.ChecksumSha256, almacenado.ClaveRelativa), cancellationToken);
        }
        catch (Exception errorPersistencia)
        {
            await CompensarSoloSiNoHayReferenciaAsync(almacenado.ClaveRelativa, errorPersistencia);
            throw;
        }
    }

    public async Task EliminarBorradorAsync(int avisoId, int entidadAcademicaId, int usuarioId,
        CancellationToken cancellationToken = default)
    {
        if (avisoId < 1 || entidadAcademicaId < 1 || usuarioId < 1)
            throw new ArgumentException("El Aviso, la Entidad Académica y la cuenta deben ser válidos.");

        var claves = await _repository.ObtenerClavesBorradorAsync(avisoId, entidadAcademicaId, usuarioId, cancellationToken);
        var cuarentena = new List<DocumentoEnCuarentena>();
        try
        {
            foreach (var clave in claves)
            {
                var documento = await _almacen.PrepararEliminacionAsync(clave, cancellationToken);
                if (documento is not null) cuarentena.Add(documento);
            }

            await _repository.EliminarBorradorAsync(avisoId, entidadAcademicaId, usuarioId, claves, cancellationToken);
        }
        catch (Exception error)
        {
            try
            {
                if (!await _repository.ExisteAvisoAsync(avisoId, CancellationToken.None))
                {
                    var erroresFinalizacionAmbigua = await CompletarCuarentenaAsync(cuarentena);
                    if (erroresFinalizacionAmbigua.Count > 0)
                        throw new AggregateException("SQL confirmó la eliminación del Aviso, pero quedaron documentos en cuarentena que requieren conciliación.",
                            [error, .. erroresFinalizacionAmbigua]);
                    throw new AggregateException("SQL confirmó la eliminación del Aviso; se completó la eliminación de sus documentos, aunque la respuesta de SQL fue ambigua.",
                        error);
                }
            }
            catch (AggregateException) { throw; }
            catch (Exception errorConsulta)
            {
                var erroresRestauracionAmbigua = await RestaurarCuarentenaAsync(cuarentena);
                throw new AggregateException("No se pudo resolver el resultado SQL del borrado; se restauraron los archivos cuando fue posible y requiere conciliación.",
                    [error, errorConsulta, .. erroresRestauracionAmbigua]);
            }
            var erroresCompensacion = await RestaurarCuarentenaAsync(cuarentena);
            if (erroresCompensacion.Count > 0)
                throw new AggregateException("Falló la eliminación del borrador y no se pudieron restaurar todos sus documentos; requiere conciliación operativa.",
                    [error, .. erroresCompensacion]);
            throw;
        }

        var erroresFinalizacion = new List<Exception>();
        foreach (var documento in cuarentena)
        {
            try { await _almacen.CompletarEliminacionAsync(documento, CancellationToken.None); }
            catch (Exception error) { erroresFinalizacion.Add(error); }
        }
        if (erroresFinalizacion.Count > 0)
            throw new AggregateException("El borrador se eliminó en SQL, pero algunos documentos quedaron en cuarentena y requieren conciliación operativa.",
                erroresFinalizacion);
    }

    private async Task<List<Exception>> RestaurarCuarentenaAsync(IReadOnlyList<DocumentoEnCuarentena> cuarentena)
    {
        var errores = new List<Exception>();
        foreach (var documento in cuarentena.Reverse())
        {
            try { await _almacen.RestaurarEliminacionAsync(documento, CancellationToken.None); }
            catch (Exception error) { errores.Add(error); }
        }
        return errores;
    }

    private async Task<List<Exception>> CompletarCuarentenaAsync(IReadOnlyList<DocumentoEnCuarentena> cuarentena)
    {
        var errores = new List<Exception>();
        foreach (var documento in cuarentena)
        {
            try { await _almacen.CompletarEliminacionAsync(documento, CancellationToken.None); }
            catch (Exception error) { errores.Add(error); }
        }
        return errores;
    }

    private async Task CompensarSoloSiNoHayReferenciaAsync(string claveRelativa, Exception errorPersistencia)
    {
        bool persistida;
        try
        {
            persistida = await _repository.ExisteClaveAlmacenamientoAsync(claveRelativa, CancellationToken.None);
        }
        catch (Exception errorConsulta)
        {
            throw new AggregateException("No se pudo confirmar si SQL guardó el documento; se conservó el archivo para conciliación.",
                errorPersistencia, errorConsulta);
        }
        if (persistida)
            throw new AggregateException("SQL conserva metadatos para el documento; se mantuvo el archivo porque el resultado del guardado fue ambiguo.",
                errorPersistencia);
        try
        {
            await _almacen.EliminarAsync(claveRelativa, CancellationToken.None);
        }
        catch (Exception errorCompensacion)
        {
            throw new AggregateException("Falló el guardado de metadatos y también la compensación del archivo; requiere conciliación operativa.",
                errorPersistencia, errorCompensacion);
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
        var copia = new MemoryStream();
        await using (contenido)
            await contenido.CopyToAsync(copia, cancellationToken);
        var checksum = SHA256.HashData(copia.GetBuffer().AsSpan(0, checked((int)copia.Length)));
        if (!CryptographicOperations.FixedTimeEquals(checksum, metadata.ChecksumSha256))
        {
            await copia.DisposeAsync();
            throw new InvalidDataException("El checksum del documento no coincide con los metadatos de SQL.");
        }
        copia.Position = 0;
        return new DescargaDocumentoAvisoMvc(copia, metadata.Nombre, metadata.Mime);
    }
}
