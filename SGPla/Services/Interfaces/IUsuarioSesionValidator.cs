using System.Security.Claims;

namespace SGPla.Services.Interfaces;

public interface IUsuarioSesionValidator
{
    Task<bool> EsSesionVigenteAsync(ClaimsPrincipal? principal, CancellationToken cancellationToken = default);
}
