using Microsoft.AspNetCore.Mvc;
using SGPla.Mappers;
using SGPla.Models;
using SGPla.Models.DTOs.Articulo;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers
{
    public class ArticulosController : Controller
    {
        private readonly IArticuloService _articuloService;

        public ArticulosController(IArticuloService articuloService)
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
        public async Task<IActionResult> Crear(CrearArticuloDTO dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _articuloService.CrearArticuloAsync(dto);
                }
                catch (ArgumentException ex)
                {
                    TempData["Error"] = ex.Message; 
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarArticuloDTO dto) 
        {
            if (ModelState.IsValid)
            {
                try
                {

                
                await _articuloService.EditarArticuloAsync(dto);
                }
                catch (ArgumentException ex)
                {
                    TempData["Error"] = ex.Message;
                }

            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _articuloService.EliminarArticuloAsync(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
