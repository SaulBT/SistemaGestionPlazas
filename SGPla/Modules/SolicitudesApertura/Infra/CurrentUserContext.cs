using System.Security.Claims;
using SGPla.Commons;
using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Ports;

namespace SGPla.Modules.SolicitudesApertura.Infra;

public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool EstaAutenticado =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public bool EsCoordinadorEa =>
        _httpContextAccessor.HttpContext?.User.IsInRole(Constantes.COORDINADOR_EA) == true;

    public string? Correo => _httpContextAccessor.HttpContext?.User
        .FindFirstValue(ClaimTypes.Email);
}
