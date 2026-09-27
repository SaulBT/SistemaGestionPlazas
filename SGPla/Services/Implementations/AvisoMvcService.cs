using SGPla.Models.DTOs.Avisos;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class AvisoMvcService : IAvisoMvcService
{
    private readonly IAvisoMvcRepository _repository;

    public AvisoMvcService(IAvisoMvcRepository repository) => _repository = repository;

    public Task<AvisoMvcPagina> BuscarAsync(int usuarioId, AvisoMvcFiltro filtro,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        if (usuarioId < 1) throw new UnauthorizedAccessException("La sesión no contiene un usuario válido.");
        if (filtro.PeriodoEscolarId is <= 0 || filtro.EntidadAcademicaId is <= 0)
            throw new ArgumentException("Los filtros de catálogo deben usar IDs positivos.");
        var estados = new HashSet<string>(StringComparer.Ordinal)
        {
            "CREADO", "EN_REVISION_DGAA", "DEVUELTO_DGAA", "AVALADO_DGAA", "FIRMADO", "PUBLICADO", "CANCELADO"
        };
        if (!string.IsNullOrWhiteSpace(filtro.Estado)
            && !estados.Contains(filtro.Estado.Trim().ToUpperInvariant()))
            throw new ArgumentException("El estado seleccionado no es válido.");
        return _repository.BuscarAsync(usuarioId, filtro, cancellationToken);
    }

    public Task<AvisoMvcNuevoDatos> ObtenerDatosNuevoAsync(int usuarioId, int? periodoEscolarId,
        int? sistemaEducativoId, CancellationToken cancellationToken = default)
    {
        if (usuarioId < 1) throw new UnauthorizedAccessException("La sesión no contiene un usuario válido.");
        if (periodoEscolarId is <= 0 || sistemaEducativoId is <= 0)
            throw new ArgumentException("Los catálogos seleccionados deben tener IDs positivos.");
        return _repository.ObtenerDatosNuevoAsync(usuarioId, periodoEscolarId, sistemaEducativoId, cancellationToken);
    }

    public Task<AvisoMvcDetalle?> ObtenerDetalleAsync(int usuarioId, int avisoId,
        CancellationToken cancellationToken = default)
    {
        if (usuarioId < 1 || avisoId < 1) throw new ArgumentException("La cuenta y el Aviso deben ser válidos.");
        return _repository.ObtenerDetalleAsync(usuarioId, avisoId, cancellationToken);
    }

    public Task<int> CrearBorradorAsync(int usuarioId, int entidadAcademicaId,
        CrearAvisoMvcDatos datos, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (usuarioId < 1 || entidadAcademicaId < 1)
            throw new UnauthorizedAccessException("La sesión no contiene un ámbito válido.");
        if (datos.PeriodoEscolarId < 1 || datos.SistemaEducativoId < 1 || datos.ArticuloId < 1)
            throw new ArgumentException("Selecciona periodo, sistema educativo y artículo vigentes.");
        var tipo = datos.TipoComunicado?.Trim().ToUpperInvariant();
        if (tipo is not ("AVISO" or "CONVOCATORIA"))
            throw new ArgumentException("El tipo de comunicado no es válido.");
        if (datos.OfertaIds is null || datos.OfertaIds.Count is < 1 or > 50
            || datos.OfertaIds.Any(id => id < 1)
            || datos.OfertaIds.Distinct().Count() != datos.OfertaIds.Count)
            throw new ArgumentException("Selecciona entre 1 y 50 Ofertas distintas y vigentes.");
        return _repository.CrearBorradorAsync(usuarioId, entidadAcademicaId,
            datos with { TipoComunicado = tipo }, cancellationToken);
    }

    public Task AgregarOfertasAsync(int usuarioId, int entidadAcademicaId, int avisoId,
        IReadOnlyList<int> ofertaIds, CancellationToken cancellationToken = default)
    {
        ValidarAmbito(usuarioId, entidadAcademicaId, avisoId);
        ValidarIdsOferta(ofertaIds);
        return _repository.AgregarOfertasAsync(usuarioId, entidadAcademicaId, avisoId, ofertaIds, cancellationToken);
    }

    public Task RetirarOfertaAsync(int usuarioId, int entidadAcademicaId, int avisoId, int avisoOfertaId,
        CancellationToken cancellationToken = default)
    {
        ValidarAmbito(usuarioId, entidadAcademicaId, avisoId);
        if (avisoOfertaId < 1) throw new ArgumentException("La Oferta asociada debe ser válida.");
        return _repository.RetirarOfertaAsync(usuarioId, entidadAcademicaId, avisoId, avisoOfertaId, cancellationToken);
    }

    public Task EnviarARevisionAsync(int usuarioId, int entidadAcademicaId, int avisoId,
        ConfigurarYEnviarAvisoMvcDatos datos, CancellationToken cancellationToken = default)
    {
        ValidarAmbito(usuarioId, entidadAcademicaId, avisoId);
        ArgumentNullException.ThrowIfNull(datos);
        if (datos.ModalidadRecepcionId < 1 || string.IsNullOrWhiteSpace(datos.Requisitos)
            || string.IsNullOrWhiteSpace(datos.CorreoContacto) || string.IsNullOrWhiteSpace(datos.NombreTitular))
            throw new ArgumentException("Completa modalidad, requisitos, correo de contacto y nombre del titular.");
        if (datos.LugarRecepcion?.Length > 500
            || datos.CorreoContacto.Length > 254 || datos.NombreTitular.Length > 200)
            throw new ArgumentException("Uno de los datos excede la longitud permitida.");
        if (datos.Horarios is null || datos.Horarios.Count is < 1 or > 100
            || datos.Horarios.Any(x => x.Fecha == default || x.HoraInicio >= x.HoraFin)
            || datos.FechaConsejoTecnico == default || datos.FechaVacantes == default)
            throw new ArgumentException("Define entre 1 y 100 horarios válidos de recepción.");
        return _repository.EnviarARevisionAsync(usuarioId, entidadAcademicaId, avisoId, datos, cancellationToken);
    }

    public Task ResolverRevisionAsync(int usuarioId, int avisoId, bool avalar, string? comentarios,
        CancellationToken cancellationToken = default)
    {
        if (usuarioId < 1 || avisoId < 1)
            throw new UnauthorizedAccessException("La cuenta y el Aviso deben ser válidos.");
        var texto = comentarios?.Trim();
        if (!avalar && string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("Indica los comentarios para devolver el Aviso.");
        return _repository.ResolverRevisionAsync(usuarioId, avisoId, avalar, texto, cancellationToken);
    }

    public Task PublicarAsync(int usuarioId, int entidadAcademicaId, int avisoId,
        PublicarAvisoMvcDatos datos, CancellationToken cancellationToken = default)
    {
        ValidarAmbito(usuarioId, entidadAcademicaId, avisoId);
        ArgumentNullException.ThrowIfNull(datos);
        var url = datos.UrlPublicacion?.Trim();
        if (datos.FechaPublicacion == default || string.IsNullOrWhiteSpace(url)
            || url.Length > 2048 || !Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("Indica una fecha de publicación y una URL HTTP o HTTPS válida.");
        return _repository.PublicarAsync(usuarioId, entidadAcademicaId, avisoId,
            datos with { UrlPublicacion = url }, cancellationToken);
    }

    public Task CancelarAsync(int usuarioId, int avisoId, CancelarAvisoMvcDatos datos,
        CancellationToken cancellationToken = default)
    {
        if (usuarioId < 1 || avisoId < 1) throw new UnauthorizedAccessException("La cuenta y el Aviso deben ser válidos.");
        ArgumentNullException.ThrowIfNull(datos);
        var motivo = datos.Motivo?.Trim();
        if (string.IsNullOrWhiteSpace(motivo) || motivo.Length > 1000)
            throw new ArgumentException("Indica un motivo de cancelación de hasta 1000 caracteres.");
        return _repository.CancelarAsync(usuarioId, avisoId, datos with { Motivo = motivo }, cancellationToken);
    }

    public Task ArchivarAsync(int usuarioId, int avisoId, CancellationToken cancellationToken = default)
    {
        if (usuarioId < 1 || avisoId < 1) throw new UnauthorizedAccessException("La cuenta y el Aviso deben ser válidos.");
        return _repository.ArchivarAsync(usuarioId, avisoId, cancellationToken);
    }

    private static void ValidarAmbito(int usuarioId, int entidadAcademicaId, int avisoId)
    {
        if (usuarioId < 1 || entidadAcademicaId < 1 || avisoId < 1)
            throw new UnauthorizedAccessException("La sesión no contiene un ámbito vigente.");
    }

    private static void ValidarIdsOferta(IReadOnlyList<int> ofertaIds)
    {
        if (ofertaIds is null || ofertaIds.Count is < 1 or > 50
            || ofertaIds.Any(id => id < 1) || ofertaIds.Distinct().Count() != ofertaIds.Count)
            throw new ArgumentException("Selecciona entre 1 y 50 Ofertas distintas y vigentes.");
    }
}
