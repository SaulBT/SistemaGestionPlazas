using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            IEnumerable<DetallesArticuloDTO>? articulos = null;

            try
            {
                articulos = await _articuloService.ObtenerTodosAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

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
                    var resultado = await _articuloService.CrearArticuloAsync(dto);

                    TempData["Success"] = $"Articulo creado exitosamente con ID: {resultado.IdArticulo}";

                }
                catch (ArgumentException ex)
                {
                    TempData["Error"] = ex.Message; 
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Buscar(string busqueda)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var resultados = await _articuloService.BuscarPorTerminoAsync(busqueda);
                      return View("Index", resultados);

                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View("Index");
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
                    var resultado = await _articuloService.EditarArticuloAsync(dto);
                    TempData["Success"] = $"Articulo creado exitosamente con ID: {resultado.IdArticulo}";

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
            try
            {
                var resultado = await _articuloService.EliminarArticuloAsync(id);
                if (resultado)
                {
                    TempData["Success"] = $"Artículo eliminado exitosamente";
                }
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
