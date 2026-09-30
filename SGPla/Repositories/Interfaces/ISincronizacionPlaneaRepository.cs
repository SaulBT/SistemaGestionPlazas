using SGPla.Models.DTOs.Planea;

namespace SGPla.Repositories.Interfaces
{
    public interface ISincronizacionPlaneaRepository
    {
        Task<IReadOnlyList<PeriodoPorSincronizar>> ObtenerPeriodosVigentesAsync(DateOnly hoy, int ventanaAnticipacionMeses, CancellationToken cancellationToken = default);
        Task<int> ContarPeriodosSinFechasAsync(CancellationToken cancellationToken = default);
        Task<int> IniciarBitacoraAsync(int idPeriodo, CancellationToken cancellationToken = default);
        Task CerrarBitacoraAsync(int idSincronizacion, string estado, int? registrosRecibidos, DatosPeriodoPlanea? datos, ResumenAplicacionPlanea? resumen, string? advertencias, string? mensajeError, CancellationToken cancellationToken = default);
        Task<int> MarcarInterrumpidasAsync(TimeSpan umbral, CancellationToken cancellationToken = default);
        Task<ResumenAplicacionPlanea> RegistrarNuevasAsync(int idPeriodo, string codigoPeriodo, int idSincronizacion, DatosPeriodoPlanea datos, CancellationToken cancellationToken = default);
    }
}
