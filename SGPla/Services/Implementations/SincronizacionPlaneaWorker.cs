using Microsoft.Extensions.Options;
using SGPla.Commons;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations
{
    public sealed class SincronizacionPlaneaWorker(IServiceScopeFactory scopeFactory, IOptions<PlaneaOpciones> opciones,
        ILogger<SincronizacionPlaneaWorker> logger) : BackgroundService
    {
        private readonly PlaneaOpciones _opciones = opciones.Value;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_opciones.Habilitada) { logger.LogInformation("La sincronización con PLANEA está deshabilitada."); return; }
            try
            {
                await Task.Delay(_opciones.RetrasoInicial, stoppingToken);
                using var timer = new PeriodicTimer(_opciones.Intervalo);
                do { await EjecutarCicloAsync(stoppingToken); } while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        }
        private async Task EjecutarCicloAsync(CancellationToken stoppingToken)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var servicio = scope.ServiceProvider.GetRequiredService<ISincronizarPeriodosVigentesService>();
                await servicio.EjecutarAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException) { logger.LogError(ex, "Falló el ciclo de sincronización con PLANEA."); }
        }
    }
}
