using Microsoft.AspNetCore.Mvc;
using SGPla.Models;
using SGPla.Services.Interfaces;
using SGPla.Services.Implementations;

namespace SGPla.Controllers
{

    public class UsuariosController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        
        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        public IActionResult Index()
        {
            return View();
        }
        /*
        public async Task<IActionResult> Index()
        {
            var usuarios = await _usuarioService.ObtenerTodosAsync();
            return View(usuarios);
        }*/
        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                await _usuarioService.CrearUsuarioAsync(usuario);
            }
            return RedirectToAction("Index");
        }*/
    }
}