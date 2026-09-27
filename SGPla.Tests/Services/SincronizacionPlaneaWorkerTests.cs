using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;

namespace SGPla.Tests.Services;

public sealed class SincronizacionPlaneaWorkerTests
{
    [Fact]
    public async Task Worker_procesa_la_cola_y_continua_despues_de_una_falla()
    {
        var canal = new CanalSincronizacionPlanea();
        var servicio = new FakeSincronizacionPlaneaService();
        using var host = new HostBuilder().ConfigureServices(services =>
        {
            services.AddSingleton(canal);
            services.AddSingleton<ISincronizacionPlaneaService>(servicio);
            services.AddSingleton<ILogger<SincronizacionPlaneaWorker>>(NullLogger<SincronizacionPlaneaWorker>.Instance);
            services.AddHostedService<SincronizacionPlaneaWorker>();
        }).Build();

        await host.StartAsync();
        Assert.True(canal.TryEnqueue(new SolicitudSincronizacionPlanea(1)));
        Assert.True(canal.TryEnqueue(new SolicitudSincronizacionPlanea(2)));
        await servicio.Completado.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await host.StopAsync();

        Assert.Equal("EXITOSA", servicio.Estados[1]);
        Assert.Equal("FALLIDA", servicio.Estados[2]);
        Assert.Equal(2, servicio.Llamadas);
    }

    private sealed class FakeSincronizacionPlaneaService : ISincronizacionPlaneaService
    {
        public Dictionary<int, string> Estados { get; } = [];
        public TaskCompletionSource Completado { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int Llamadas { get; private set; }
        public Task<int> SolicitarAsync(int periodoEscolarId, CancellationToken cancellationToken = default) => Task.FromResult(periodoEscolarId);
        public Task<int> SincronizarAsync(int periodoEscolarId, CancellationToken cancellationToken = default) => Task.FromResult(periodoEscolarId);
        public Task<EstadoSincronizacionPlanea?> ObtenerEstadoAsync(int sincronizacionId, CancellationToken cancellationToken = default) => Task.FromResult<EstadoSincronizacionPlanea?>(null);
        public Task MarcarEnProcesoHuerfanasAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task EjecutarRegistradaAsync(int sincronizacionId, CancellationToken cancellationToken = default)
        {
            Llamadas++;
            if (sincronizacionId == 2)
            {
                Estados[sincronizacionId] = "FALLIDA";
                if (Llamadas == 2) Completado.TrySetResult();
                throw new InvalidOperationException("fallo de prueba");
            }
            Estados[sincronizacionId] = "EXITOSA";
            if (Llamadas == 2) Completado.TrySetResult();
            return Task.CompletedTask;
        }
    }
}
