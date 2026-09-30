using SGPla.Models.DTOs.Planea;

namespace SGPla.Repositories.Interfaces
{
    public interface IProgramacionPlaneaRepository
    {
        Task<UltimaSincronizacionPlaneaDTO?> ObtenerUltimaSincronizacionAsync(int? idPeriodo, CancellationToken cancellationToken = default);
        Task<(List<CopiaProgramacionPlaneaDTO> Copias, int Total)> ObtenerCopiasAsync(FiltroProgramacionPlaneaDTO filtro, CancellationToken cancellationToken = default);
        Task<PeriodoPorSincronizar?> ObtenerPeriodoAsync(int idPeriodo, CancellationToken cancellationToken = default);
    }
}
