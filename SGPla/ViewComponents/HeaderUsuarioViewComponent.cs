using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models.Components;
using System.Security.Claims;

namespace SGPla.ViewComponents
{
    public class HeaderUsuarioViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var usuario = HttpContext.User;
            var estaAutenticado = usuario.Identity?.IsAuthenticated == true;

            var model = new HeaderUsuarioModel
            {
                EstaAutenticado = estaAutenticado,
                Nombre = usuario.FindFirst(ClaimTypes.Name)?.Value,
                Rol = HttpContext.Session.GetString("RolActivo")
                      ?? usuario.FindFirst(ClaimTypes.Role)?.Value,
                TieneSuperUsuario = usuario.IsInRole(Constantes.SUPERUSUARIO),
            };

            if (estaAutenticado && model.TieneSuperUsuario)
            {
                model.RolActivoEsSuperUsuario = model.Rol == Constantes.SUPERUSUARIO;
            }

            return View(model);
        }
    }

    
}