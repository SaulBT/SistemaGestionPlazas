namespace SGPla.Services.Interfaces;

public interface ISincronizacionPlaneaService
{
    Task<int> SolicitarAsync(int periodoEscolarId, CancellationToken cancellationToken = default);
    Task EjecutarRegistradaAsync(int sincronizacionId, CancellationToken cancellationToken = default);
    Task<EstadoSincronizacionPlanea?> ObtenerEstadoAsync(int sincronizacionId, CancellationToken cancellationToken = default);
    Task MarcarEnProcesoHuerfanasAsync(CancellationToken cancellationToken = default);
    Task<int> SincronizarAsync(int periodoEscolarId, CancellationToken cancellationToken = default);
}

public sealed record EstadoSincronizacionPlanea(
    int Id, int PeriodoEscolarId, string Estado, DateTime IniciadaEn, DateTime? FinalizadaEn,
    int RegistrosRecibidos, int RegistrosIgnorados, int SesionesGeneradas,
    int DuplicadosDescartados, int Advertencias, string? MensajeError);
