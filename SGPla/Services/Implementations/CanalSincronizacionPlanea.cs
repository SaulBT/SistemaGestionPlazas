using System.Threading.Channels;

namespace SGPla.Services.Implementations;

public sealed record SolicitudSincronizacionPlanea(int SincronizacionId);

public sealed class CanalSincronizacionPlanea
{
    private readonly Channel<SolicitudSincronizacionPlanea> _channel = Channel.CreateBounded<SolicitudSincronizacionPlanea>(
        new BoundedChannelOptions(10)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });

    public bool TryEnqueue(SolicitudSincronizacionPlanea solicitud) => _channel.Writer.TryWrite(solicitud);
    public IAsyncEnumerable<SolicitudSincronizacionPlanea> LeerAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
