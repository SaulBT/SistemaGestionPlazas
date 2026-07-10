using Microsoft.AspNetCore.Mvc;
using SGPla.Models.Components;
using SGPla.Models.ViewModels.Avisos;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers
{
    public class AvisosController : Controller
    {
        private readonly IAvisoService _avisoService;
        private readonly IPeriodoEscolarService _periodoService;
        private readonly ILogger<AvisosController> _logger;
        private int _paginaActual = 1;

        public AvisosController(IAvisoService avisoService, IPeriodoEscolarService periodoService, ILogger<AvisosController> logger)
        {
            _avisoService = avisoService;
            _periodoService = periodoService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? busqueda, int? idPeriodo, DateOnly? fechaInicio, DateOnly? fechaFin, int cantidad = 10)
        {
            var periodos = await _periodoService.ObtenerTodosAsync();
            var periodosCombo = periodos.
                Select(p => new OptionModel
                {
                    Value = p.IdPeriodoEscolar.ToString(),
                    Text = p.PeriodoMostrar,
                    Selected = idPeriodo.HasValue && p.IdPeriodoEscolar == idPeriodo.Value
                })
                .ToList();

            return View(new IndexViewModel
            {
                Periodos = periodosCombo,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                IdPeriodoSeleccionado = idPeriodo,
                PaginaActual = _paginaActual,
                CantidadPorPagina = cantidad,
            });
        }
    }
}
