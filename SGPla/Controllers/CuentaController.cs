using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models.ViewModels.Login;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers;

[Route("Cuenta")]
[Authorize(Policy = PoliticasAutorizacion.SuperUsuario)]
public sealed class CuentaController : Controller
{
    private readonly IAuthService _authService;

    public CuentaController(IAuthService authService) => _authService = authService;

    [HttpGet("CambiarContrasena")]
    public IActionResult CambiarContrasena()
    {
        if (User.FindFirstValue("DebeCambiarContrasena") != "true")
            return RedirectToAction("Index", "Usuarios");

        return View(new CambiarContrasenaSuperusuarioViewModel());
    }

    [HttpPost("CambiarContrasena")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarContrasena(CambiarContrasenaSuperusuarioViewModel model,
        CancellationToken cancellationToken)
    {
        if (User.FindFirstValue("DebeCambiarContrasena") != "true")
            return RedirectToAction("Index", "Usuarios");

        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId) || usuarioId <= 0)
            return Forbid();

        if (!ModelState.IsValid)
            return View(model);

        var cambioRealizado = await _authService.CambiarContrasenaSuperusuarioAsync(usuarioId,
            model.ContrasenaActual, model.ContrasenaNueva, cancellationToken);
        if (!cambioRealizado)
        {
            ModelState.AddModelError(string.Empty, "No se pudo validar la contraseña actual o guardar la nueva.");
            return View(model);
        }

        var identity = new ClaimsIdentity(User.Identity);
        var oldClaim = identity.FindFirst("DebeCambiarContrasena");
        if (oldClaim is not null)
            identity.RemoveClaim(oldClaim);
        identity.AddClaim(new Claim("DebeCambiarContrasena", "false"));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = false });

        TempData["Success"] = "La contraseña se actualizó correctamente.";
        return RedirectToAction("Index", "Usuarios");
    }
}
