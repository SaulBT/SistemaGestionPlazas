using Microsoft.AspNetCore.Mvc;
using SGPla.Mappers;
using SGPla.Models;
using SGPla.Models.DTOs.Articulo;
using SGPla.Services.Interfaces;

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
            var dtos = articulos.Select(a => ArticuloMapper.ToFormularioDTO(a));
            return View(dtos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FormularioArticuloDTO dto)
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
        public async Task<IActionResult> Edit(FormularioArticuloDTO dto) 
        {
            if (ModelState.IsValid)
            {
               // await _articuloService.ActualizarArticuloAsync(dto); 
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _articuloService.EliminarArticuloAsync(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
