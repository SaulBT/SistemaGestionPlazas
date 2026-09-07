using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models.ViewModels.Login;
using SGPla.Services.Interfaces;
using System.Security.Claims;

namespace SGPla.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly IAuthService _authService;

        public LoginController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Index(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                string? rol = User.FindFirst(ClaimTypes.Role)?.Value;

                return rol switch
                {
                    Constantes.COORDINADOR_DGAA => RedirectToAction("Index", "ProgramacionesAcademicas"),
                    Constantes.COORDINADOR_EA => RedirectToAction("Index", "ProgramacionesAcademicas"),
                    Constantes.SUPERUSUARIO => RedirectToAction("Index", "Usuarios"),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);


            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            var resultado = await _authService.LoginAsync(model.Correo, model.Contrasena);

            if (!resultado.Exitoso)
            {
                TempData["Error"] = resultado.MensajeError;

                return View(model);
            }

            var usuario = resultado.Usuario!;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.NombreCompleto),
                new(ClaimTypes.Email, usuario.Correo),
                new(ClaimTypes.Role, usuario.Rol),
            };

            if (usuario.EntidadAcademicaId is not null)
                claims.Add(new Claim("EntidadAcademicaId", usuario.EntidadAcademicaId.ToString()!));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                 CookieAuthenticationDefaults.AuthenticationScheme,
                 principal,
                 new AuthenticationProperties
                 {
                     IsPersistent = false
                 });

            // El inicio depende del único rol efectivo de la sesión. No se reutiliza
            // ReturnUrl porque podría apuntar a una ruta de otro rol.
            return RedirigirInicioSegunRol(usuario.Rol);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Login");
        }

        private IActionResult RedirigirInicioSegunRol(string rol) => rol switch
        {
            Constantes.SUPERUSUARIO => RedirectToAction("Index", "Usuarios"),
            Constantes.COORDINADOR_DGAA => RedirectToAction("Index", "ProgramacionesAcademicas"),
            Constantes.COORDINADOR_EA => RedirectToAction("Index", "ProgramacionesAcademicas"),
            _ => RedirectToAction("Index", "Home")
        };

    }
}
