using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models.DTOs.Auth;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers;

[Route("Administracion/Superusuarios")]
[Authorize(Policy = PoliticasAutorizacion.SuperUsuario)]
public sealed class AdministracionSuperusuariosController : Controller
{
    private readonly ISuperusuarioAdminService _service;

    public AdministracionSuperusuariosController(ISuperusuarioAdminService service) => _service = service;

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!ObtenerUsuarioId(out var actorId)) return Forbid();
        try { return View(await _service.ListarAsync(actorId, cancellationToken)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost("{id:int}/Restablecer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restablecer(int id, string? contrasenaTemporal, string? confirmarContrasenaTemporal,
        CancellationToken cancellationToken)
    {
        if (!ObtenerUsuarioId(out var actorId)) return Forbid();
        if (string.IsNullOrEmpty(contrasenaTemporal) || !string.Equals(contrasenaTemporal,
                confirmarContrasenaTemporal, StringComparison.Ordinal))
        {
            TempData["Error"] = "La contraseña temporal está vacía o sus confirmaciones no coinciden.";
            return RedirectToAction(nameof(Index));
        }
        try
        {
            await _service.RestablecerContrasenaTemporalAsync(actorId, id, contrasenaTemporal, cancellationToken);
            TempData["Success"] = "Se guardó la contraseña temporal. El Superusuario deberá cambiarla al iniciar sesión.";
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception exception) when (exception is ArgumentException or KeyNotFoundException or InvalidOperationException)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/Desactivar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(int id, CancellationToken cancellationToken)
    {
        if (!ObtenerUsuarioId(out var actorId)) return Forbid();
        try
        {
            await _service.DesactivarAsync(actorId, id, cancellationToken);
            TempData["Success"] = "Se desactivó el Superusuario.";
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception exception) when (exception is ArgumentException or KeyNotFoundException or InvalidOperationException)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private bool ObtenerUsuarioId(out int id) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out id) && id > 0;
}
