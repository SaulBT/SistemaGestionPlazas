using Microsoft.AspNetCore.Mvc;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers
{
    public class ProgramasEducativosController : Controller
    {
        private readonly IProgramaEducativoService _programaEducativoService;
        public ProgramasEducativosController(IProgramaEducativoService programaEducativoService)
        {
            _programaEducativoService = programaEducativoService;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<DetallesProgramaEducativoDTO>? programasEducativos = null;
            try
            {
                programasEducativos = await _programaEducativoService.ObtenerTodosAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return View(programasEducativos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearProgramaEducativoDTO dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    dto.IdEntidadAcademica = 1;
                    var resultado = await _programaEducativoService.CrearAsync(dto);
                    TempData["Success"] = $"Programa Educativo creado exitosamente con ID: {resultado.IdProgramaEducativo}";
                }
                catch (ArgumentException ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        public async Task<IActionResult> Buscar(string busqueda)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var filtro = new BuscarProgramaEducativoDTO { Nombre = busqueda };
                    var resultados = await _programaEducativoService.BuscarPorFiltroAsync(filtro);
                    return View("Index", resultados);
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
        public async Task<IActionResult> Editar(EditarProgramaEducativoDTO dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var resultado = await _programaEducativoService.EditarAsync(dto);
                    TempData["Success"] = $"Programa Educativo editado exitosamente con ID: {resultado.IdProgramaEducativo}";
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
                var resultado = await _programaEducativoService.EliminarAsync(id);
                if (resultado)
                    TempData["Success"] = $"Programa Educativo eliminado exitosamente con ID: {id}";
                else
                    TempData["Error"] = $"No se pudo eliminar el Programa Educativo con ID: {id}";
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));

        }
    }
}
