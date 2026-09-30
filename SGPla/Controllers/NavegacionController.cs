using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers
{
    [Authorize]
    public class NavegacionController : Controller
    {
        private readonly IEstadoNavegacion _estado;

        public NavegacionController(IEstadoNavegacion estado)
        {
            _estado = estado;
        }

        [HttpGet]
        public IActionResult Regresar(string? fallback = null)
        {
            var url = _estado.PopRetorno(fallback ?? Url.Action("Index", "Home")!);
            return Redirect(url);
        }
    }
}
