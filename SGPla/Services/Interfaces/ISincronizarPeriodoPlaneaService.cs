using SGPla.Models.DTOs.Planea;

namespace SGPla.Services.Interfaces
{
    public interface ISincronizarPeriodoPlaneaService
    {
        Task<ResultadoSincronizacionPlanea> SincronizarAsync(PeriodoPorSincronizar periodo, CancellationToken cancellationToken = default);
    }
}
