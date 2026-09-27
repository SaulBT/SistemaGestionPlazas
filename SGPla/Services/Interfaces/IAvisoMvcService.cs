using SGPla.Models.DTOs.Avisos;

namespace SGPla.Services.Interfaces;

public interface IAvisoMvcService
{
    Task<AvisoMvcPagina> BuscarAsync(int usuarioId, AvisoMvcFiltro filtro,
        CancellationToken cancellationToken = default);

    Task<AvisoMvcNuevoDatos> ObtenerDatosNuevoAsync(int usuarioId, int? periodoEscolarId,
        int? sistemaEducativoId, CancellationToken cancellationToken = default);

    Task<AvisoMvcDetalle?> ObtenerDetalleAsync(int usuarioId, int avisoId,
        CancellationToken cancellationToken = default);

    Task<int> CrearBorradorAsync(int usuarioId, int entidadAcademicaId,
        CrearAvisoMvcDatos datos, CancellationToken cancellationToken = default);

    Task AgregarOfertasAsync(int usuarioId, int entidadAcademicaId, int avisoId,
        IReadOnlyList<int> ofertaIds, CancellationToken cancellationToken = default);

    Task RetirarOfertaAsync(int usuarioId, int entidadAcademicaId, int avisoId, int avisoOfertaId,
        CancellationToken cancellationToken = default);

    Task EnviarARevisionAsync(int usuarioId, int entidadAcademicaId, int avisoId,
        ConfigurarYEnviarAvisoMvcDatos datos, CancellationToken cancellationToken = default);

    Task ResolverRevisionAsync(int usuarioId, int avisoId, bool avalar, string? comentarios,
        CancellationToken cancellationToken = default);

    Task PublicarAsync(int usuarioId, int entidadAcademicaId, int avisoId,
        PublicarAvisoMvcDatos datos, CancellationToken cancellationToken = default);

    Task CancelarAsync(int usuarioId, int avisoId, CancelarAvisoMvcDatos datos,
        CancellationToken cancellationToken = default);

    Task ArchivarAsync(int usuarioId, int avisoId, CancellationToken cancellationToken = default);
}
