using Microsoft.AspNetCore.Mvc;
using SGPla.Models.DTOs.PeriodoEscolar;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers
{
    public class PeriodosEscolaresController : Controller
    {
        private readonly IPeriodoEscolarService _periodoEscolarService;

        public PeriodosEscolaresController(IPeriodoEscolarService periodoEscolarService)
        {
            _periodoEscolarService = periodoEscolarService;
        }

        public async Task<IActionResult> Index(BuscarPeriodoEscolarDTO filtro)
        {
            var programasEducativos = await _periodoEscolarService.BuscarPorFiltroAsync(filtro);

            return View(programasEducativos);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearPeriodoEscolarDTO dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var resultado = await _periodoEscolarService.CrearAsync(dto);
                    TempData["Success"] = $"Periodo Escolar creado exitosamente con ID: {resultado.IdPeriodoEscolar}";
                }
                catch (ArgumentException ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));

        }


        [HttpGet]
        public async Task<IActionResult> Buscar(BuscarPeriodoEscolarDTO filtro)
        {
            try
            {
                var resultados = await _periodoEscolarService.BuscarPorFiltroAsync(filtro);


                return View("Index", resultados);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarPeriodoEscolarDTO dto)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    var resultado = await _periodoEscolarService.EditarAsync(dto);
                    TempData["Success"] = $"Periodo Escolar editado exitosamente con ID: {resultado.IdPeriodoEscolar}";
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
                var resultado = await _periodoEscolarService.EliminarAsync(id);
                if (resultado)
                    TempData["Success"] = $"Periodo Escolar eliminado exitosamente con ID: {id}";
                else
                    TempData["Error"] = $"No se pudo eliminar el Periodo Escolar con ID: {id}";
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));

        }
    }
}
