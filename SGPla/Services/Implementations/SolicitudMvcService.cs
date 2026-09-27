using System.Net.Mail;
using System.Security.Cryptography;
using SGPla.Models.DTOs.Solicitudes;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class SolicitudMvcService : ISolicitudMvcService
{
    private readonly ISolicitudMvcRepository _repository;
    private readonly IAlmacenDocumentos? _almacen;

    public SolicitudMvcService(ISolicitudMvcRepository repository) => _repository = repository;

    public SolicitudMvcService(ISolicitudMvcRepository repository, IAlmacenDocumentos almacen)
    {
        _repository = repository;
        _almacen = almacen;
    }

    public Task<SolicitudMvcListado?> ListarPorAvisoAsync(int usuarioId, int entidadAcademicaId, int avisoId,
        CancellationToken cancellationToken = default) =>
        _repository.ListarPorAvisoAsync(usuarioId, entidadAcademicaId, avisoId, cancellationToken);

    public Task<int> RegistrarAsync(int usuarioId, int entidadAcademicaId, RegistrarSolicitudMvcDatos datos,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        ValidarIdentidad(datos.Nombre, datos.Correo, datos.PuestoActual, datos.DescripcionPerfil);
        ValidarFormaciones(datos.Formaciones);
        if (datos.AvisoOfertaId < 1) throw new ArgumentException("Seleccione una vacante publicada.");
        if (datos.Observaciones?.Length > 4000) throw new ArgumentException("Las observaciones no pueden exceder 4000 caracteres.");
        var normalizado = datos with
        {
            Nombre = datos.Nombre.Trim(),
            Correo = NormalizarCorreo(datos.Correo),
            PuestoActual = NormalizarOpcional(datos.PuestoActual),
            DescripcionPerfil = datos.DescripcionPerfil.Trim(),
            Observaciones = NormalizarOpcional(datos.Observaciones),
            Formaciones = NormalizarFormaciones(datos.Formaciones)
        };
        return _repository.RegistrarAsync(usuarioId, entidadAcademicaId, normalizado, cancellationToken);
    }

    public Task<SolicitudMvcDetalle?> ObtenerDetalleAsync(int usuarioId, int entidadAcademicaId, int avisoId,
        int solicitudId, CancellationToken cancellationToken = default) =>
        _repository.ObtenerDetalleAsync(usuarioId, entidadAcademicaId, avisoId, solicitudId, cancellationToken);

    public Task ActualizarPerfilAsync(int usuarioId, int entidadAcademicaId, int avisoId, int solicitudId,
        EditarPerfilSolicitudMvcDatos datos, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        ValidarIdentidad(datos.Nombre, datos.Correo, datos.PuestoActual, datos.DescripcionPerfil);
        ValidarFormaciones(datos.Formaciones);
        return _repository.ActualizarPerfilAsync(usuarioId, entidadAcademicaId, avisoId, solicitudId,
            new EditarPerfilSolicitudMvcDatos(datos.Nombre.Trim(), NormalizarCorreo(datos.Correo),
                NormalizarOpcional(datos.PuestoActual), datos.DescripcionPerfil.Trim(), NormalizarFormaciones(datos.Formaciones)),
            cancellationToken);
    }

    public Task ResolverAsync(int usuarioId, int entidadAcademicaId, int avisoId, int solicitudId,
        ResolverSolicitudMvcDatos datos, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var motivo = NormalizarOpcional(datos.MotivoNoAdmision);
        if (!datos.Admitir && (string.IsNullOrWhiteSpace(motivo) || motivo.Length > 1000))
            throw new ArgumentException("Indique un motivo de no admisión de hasta 1000 caracteres.");
        return _repository.ResolverAsync(usuarioId, entidadAcademicaId, avisoId, solicitudId,
            datos with { MotivoNoAdmision = motivo }, cancellationToken);
    }

    public Task RetirarAsync(int usuarioId, int entidadAcademicaId, int avisoId, int solicitudId,
        string? motivo, CancellationToken cancellationToken = default)
    {
        var normalizado = NormalizarOpcional(motivo);
        if (normalizado?.Length > 1000) throw new ArgumentException("El motivo de retiro no puede exceder 1000 caracteres.");
        return _repository.RetirarAsync(usuarioId, entidadAcademicaId, avisoId, solicitudId, normalizado, cancellationToken);
    }

    public async Task<int> AgregarDocumentoAsync(int usuarioId, int entidadAcademicaId, int avisoId,
        int solicitudId, int tipoDocumentoId, int? documentoAspiranteId, Stream contenido, string nombre, string mime, long tamanoDeclarado,
        CancellationToken cancellationToken = default)
    {
        if (usuarioId < 1 || entidadAcademicaId < 1 || avisoId < 1 || solicitudId < 1 || tipoDocumentoId < 1)
            throw new ArgumentException("El usuario, ámbito, Solicitud y tipo de documento deben ser válidos.");
        if (documentoAspiranteId is <= 0)
            throw new ArgumentException("El documento que se va a versionar no es válido.");
        var almacen = _almacen ?? throw new InvalidOperationException("El almacenamiento de documentos no está configurado.");
        var guardado = await almacen.GuardarAspiranteAsync(contenido, nombre, mime, tamanoDeclarado, cancellationToken);
        try
        {
            return await _repository.AgregarDocumentoAsync(usuarioId, entidadAcademicaId, avisoId, solicitudId,
                new DocumentoAspiranteMvcDatos(tipoDocumentoId, documentoAspiranteId, guardado.Nombre, guardado.Mime,
                    guardado.Tamano, guardado.ChecksumSha256, guardado.ClaveRelativa), cancellationToken);
        }
        catch (Exception errorPersistencia)
        {
            bool persistida;
            try
            {
                persistida = await _repository.ExisteClaveDocumentoAsync(guardado.ClaveRelativa, CancellationToken.None);
            }
            catch (Exception errorConsulta)
            {
                throw new AggregateException("No se pudo confirmar si SQL guardó el documento de Aspirante; se conservó el archivo para conciliación.",
                    errorPersistencia, errorConsulta);
            }
            if (persistida)
                throw new AggregateException("SQL conserva metadatos para el documento de Aspirante; se mantuvo el archivo porque el resultado fue ambiguo.",
                    errorPersistencia);
            try { await almacen.EliminarAsync(guardado.ClaveRelativa, CancellationToken.None); }
            catch (Exception errorCompensacion)
            {
                throw new AggregateException("Falló el guardado SQL del documento de Aspirante y también su compensación; requiere conciliación operativa.",
                    errorPersistencia, errorCompensacion);
            }
            throw;
        }
    }

    public async Task<DescargaDocumentoSolicitudMvc?> AbrirDocumentoAsync(int usuarioId, int entidadAcademicaId,
        int avisoId, int solicitudId, int versionDocumentoId, CancellationToken cancellationToken = default)
    {
        var almacen = _almacen ?? throw new InvalidOperationException("El almacenamiento de documentos no está configurado.");
        var metadata = await _repository.ObtenerDocumentoAsync(usuarioId, entidadAcademicaId, avisoId,
            solicitudId, versionDocumentoId, cancellationToken);
        if (metadata is null) return null;
        var contenido = await almacen.AbrirLecturaAsync(metadata.Value.ClaveRelativa, cancellationToken);
        if (contenido is null) throw new FileNotFoundException("El archivo asociado al documento no está disponible.");
        var copia = new MemoryStream();
        await using (contenido)
            await contenido.CopyToAsync(copia, cancellationToken);
        var checksum = SHA256.HashData(copia.GetBuffer().AsSpan(0, checked((int)copia.Length)));
        if (!CryptographicOperations.FixedTimeEquals(checksum, metadata.Value.Checksum))
        {
            await copia.DisposeAsync();
            throw new InvalidDataException("El checksum del documento no coincide con los metadatos de SQL.");
        }
        copia.Position = 0;
        return new DescargaDocumentoSolicitudMvc(copia, metadata.Value.Nombre, metadata.Value.Mime);
    }

    private static void ValidarIdentidad(string? nombre, string? correo, string? puesto, string? descripcion)
    {
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > 200)
            throw new ArgumentException("El nombre es obligatorio y no puede exceder 200 caracteres.");
        if (string.IsNullOrWhiteSpace(correo) || correo.Trim().Length > 254
            || !MailAddress.TryCreate(correo.Trim(), out var direccion)
            || !string.Equals(direccion.Address, correo.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Capture un correo electrónico válido de hasta 254 caracteres.");
        if (puesto?.Trim().Length > 200) throw new ArgumentException("El puesto actual no puede exceder 200 caracteres.");
        if (string.IsNullOrWhiteSpace(descripcion) || descripcion.Trim().Length > 10000)
            throw new ArgumentException("La descripción del perfil es obligatoria y no puede exceder 10000 caracteres.");
    }

    private static void ValidarFormaciones(IReadOnlyList<FormacionSolicitudMvcDatos>? formaciones)
    {
        if (formaciones is null || formaciones.Count is 0 or > 30)
            throw new ArgumentException("Capture entre una y treinta formaciones académicas.");
        if (formaciones.Any(x => x.GradoAcademicoId < 1 || string.IsNullOrWhiteSpace(x.Descripcion)
                                 || x.Descripcion.Trim().Length > 500))
            throw new ArgumentException("Cada formación requiere un grado académico y una descripción de hasta 500 caracteres.");
    }

    private static IReadOnlyList<FormacionSolicitudMvcDatos> NormalizarFormaciones(
        IReadOnlyList<FormacionSolicitudMvcDatos> formaciones) => formaciones
        .Select(x => x with { Descripcion = x.Descripcion.Trim() }).ToList();

    private static string NormalizarCorreo(string correo) => correo.Trim().ToLowerInvariant();

    private static string? NormalizarOpcional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
