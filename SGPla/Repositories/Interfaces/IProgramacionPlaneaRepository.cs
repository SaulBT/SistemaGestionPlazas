using SGPla.Models.DTOs.Planea;

namespace SGPla.Repositories.Interfaces
{
    public interface IProgramacionPlaneaRepository
    {
        Task<UltimaSincronizacionPlaneaDTO?> ObtenerUltimaSincronizacionAsync(int? idPeriodo, CancellationToken cancellationToken = default);
        Task<(List<CopiaProgramacionPlaneaDTO> Copias, int Total, int Pagina)> ObtenerCopiasAsync(FiltroProgramacionPlaneaDTO filtro, CancellationToken cancellationToken = default);
        /// Todas las copias enlazadas del plan × periodo (sin paginar), Pendientes primero.
        Task<List<CopiaProgramacionPlaneaDTO>> ObtenerCopiasParaAprobarAsync(int idPlanEstudios, int idPeriodo, string? busqueda, CancellationToken cancellationToken = default);
        Task<ResumenAprobacionPlaneaDTO> ObtenerResumenAprobacionAsync(int idPlanEstudios, int idPeriodo, CancellationToken cancellationToken = default);
        /// Aprueba (crea la Oferta) las copias pendientes indicadas y descarta el resto de las pendientes.
        Task<ResultadoAprobacionPlaneaDTO> AprobarAsync(int idPlanEstudios, int idPeriodo, IReadOnlyCollection<int> idsAprobados, string revisadoPor, CancellationToken cancellationToken = default);
        /// Solo Descartada → Pendiente.
        Task<bool> RestaurarAsync(int idExperienciaEducativaPeriodo, CancellationToken cancellationToken = default);
        Task<EncabezadoProgramacionPlaneaDTO?> ObtenerEncabezadoAsync(int idPlanEstudios, int idPeriodo, CancellationToken cancellationToken = default);
        Task<PeriodoPorSincronizar?> ObtenerPeriodoAsync(int idPeriodo, CancellationToken cancellationToken = default);
    }
}
