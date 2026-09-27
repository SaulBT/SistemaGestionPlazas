using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class SincronizacionPlaneaWorker : BackgroundService
{
    private readonly CanalSincronizacionPlanea _canal;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SincronizacionPlaneaWorker> _logger;

    public SincronizacionPlaneaWorker(CanalSincronizacionPlanea canal,
        IServiceScopeFactory scopeFactory, ILogger<SincronizacionPlaneaWorker> logger)
    {
        _canal = canal;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using (var startupScope = _scopeFactory.CreateAsyncScope())
        {
            var service = startupScope.ServiceProvider.GetRequiredService<ISincronizacionPlaneaService>();
            await service.MarcarEnProcesoHuerfanasAsync(stoppingToken);
        }

        await foreach (var solicitud in _canal.LeerAsync(stoppingToken))
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            try
            {
                var service = scope.ServiceProvider.GetRequiredService<ISincronizacionPlaneaService>();
                await service.EjecutarRegistradaAsync(solicitud.SincronizacionId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falló la sincronización PLANEA {SincronizacionId} en segundo plano.",
                    solicitud.SincronizacionId);
            }
        }
    }
}
