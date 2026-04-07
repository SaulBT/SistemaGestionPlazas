using Microsoft.AspNetCore.Mvc;
using SGPla.Models;
using SGPla.Services;

namespace SGPla.Controllers
{
    public class ArticuloController : Controller
    {
        private readonly IArticuloService _articuloService;

        public ArticuloController(IArticuloService articuloService)
        {
            _articuloService = articuloService;
        }

        public async Task<IActionResult> Index()
        {
            var articulos = await _articuloService.ObtenerTodosAsync();
            return View(articulos);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Articulo articulo)
        {
            if (ModelState.IsValid)
            {
                await _articuloService.CrearArticuloAsync(articulo);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var articulo = await _articuloService.ObtenerArticuloPorIdAsync(id);
            if (articulo is null)
            {
                return NotFound();
            }
            return View(articulo);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Articulo articulo)
        {

            if (ModelState.IsValid)
            {
                await _articuloService.ActualizarArticuloAsync(articulo);
                return RedirectToAction(nameof(Index));
            }
            return View(articulo);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _articuloService.EliminarArticuloAsync(id);
            return RedirectToAction(nameof(Index));

        }

    }
}
