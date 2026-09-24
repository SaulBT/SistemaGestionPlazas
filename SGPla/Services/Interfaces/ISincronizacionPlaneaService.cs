namespace SGPla.Services.Interfaces;

public interface ISincronizacionPlaneaService
{
    Task<int> SincronizarAsync(int periodoEscolarId, CancellationToken cancellationToken = default);
}
