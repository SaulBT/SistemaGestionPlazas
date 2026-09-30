using SGPla.Models.DTOs.Planea;

namespace SGPla.Services.Interfaces
{
    public interface ISincronizarPeriodosVigentesService
    {
        Task<IReadOnlyList<ResultadoSincronizacionPlanea>> EjecutarAsync(CancellationToken cancellationToken = default);
    }
}
