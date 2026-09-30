using SGPla.Models.Components;
using SGPla.Models.DTOs.Planea;

namespace SGPla.Repositories.Interfaces
{
    public interface IConsultaPlaneaRepository
    {
        Task<(List<BitacoraPlaneaDTO> Items, int Total)> ObtenerBitacoraAsync(FiltroBitacoraPlaneaDTO filtro, CancellationToken cancellationToken = default);
        Task<(List<CopiaPlaneaFilaDTO> Items, int Total)> ObtenerCopiasAsync(FiltroCopiaPlaneaDTO filtro, CancellationToken cancellationToken = default);
        Task<DetalleCopiaPlaneaDTO?> ObtenerDetalleCopiaAsync(int idExperienciaEducativaPeriodo, CancellationToken cancellationToken = default);
        Task<List<OptionModel>> ObtenerPeriodosConSincronizacionAsync(CancellationToken cancellationToken = default);
        Task<List<OptionModel>> ObtenerPlanesConCopiasAsync(int? idPeriodo, CancellationToken cancellationToken = default);
        Task<List<OptionModel>> ObtenerRegionesAsync(CancellationToken cancellationToken = default);
    }
}
