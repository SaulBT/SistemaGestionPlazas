using SGPla.Models.DTOs.Auth;

namespace SGPla.Services.Interfaces;

public interface ISuperusuarioAdminService
{
    Task<IReadOnlyList<SuperusuarioAdministrable>> ListarAsync(int actorId,
        CancellationToken cancellationToken = default);

    Task RestablecerContrasenaTemporalAsync(int actorId, int usuarioId, string contrasenaTemporal,
        CancellationToken cancellationToken = default);

    Task DesactivarAsync(int actorId, int usuarioId, CancellationToken cancellationToken = default);
}
