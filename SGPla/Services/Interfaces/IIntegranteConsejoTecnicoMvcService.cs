using SGPla.Models.DTOs.IntegranteCt;

namespace SGPla.Services.Interfaces;

public interface IIntegranteConsejoTecnicoMvcService
{
    Task<IntegranteConsejoTecnicoMvcPagina> BuscarAsync(int usuarioId, int entidadAcademicaId,
        IntegranteConsejoTecnicoMvcFiltro filtro, CancellationToken cancellationToken = default);

    Task CrearAsync(int usuarioId, int entidadAcademicaId, IntegranteConsejoTecnicoMvcCambio cambio,
        CancellationToken cancellationToken = default);

    Task CambiarVigenciaAsync(int usuarioId, int entidadAcademicaId, int integranteId,
        IntegranteConsejoTecnicoMvcCambio cambio, CancellationToken cancellationToken = default);

    Task EliminarCapturaErroneaAsync(int usuarioId, int entidadAcademicaId, int integranteId,
        CancellationToken cancellationToken = default);
}
