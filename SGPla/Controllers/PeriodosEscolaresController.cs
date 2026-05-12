using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SGPla.Models.DTOs.Articulo;
using SGPla.Models.DTOs.PeriodoEscolar;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Models.ViewModels.PeriodosEscolares;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers
{
    public class PeriodosEscolaresController : Controller
    {
        private readonly IPeriodoEscolarService _periodoEscolarService;
        private readonly ILogger<PeriodosEscolaresController> _logger;

        public PeriodosEscolaresController(IPeriodoEscolarService periodoEscolarService, ILogger<PeriodosEscolaresController> logger)
        {
            _periodoEscolarService = periodoEscolarService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? anio, string? periodo, int pagina = 1, int cantidad = 10)
        {
            return View(new IndexViewModel
            {
                Table = await LlenarTabla(anio, periodo, pagina, cantidad),
                Anio = anio,
                Periodo = periodo,
                PaginaActual = pagina,
                CantidadPorPagina = cantidad
            });
        }


        private async Task<TableModel> LlenarTabla(int? anio, string? periodo, int pagina = 1, int cantidad = 10)
        {
            try
            {
                var filtro = new BuscarPeriodoEscolarDTO
                {
                    Anio = anio,
                    Periodo = periodo,
                    Pagina = pagina,
                    Cantidad = cantidad
                };

                var resultado = await _periodoEscolarService.BuscarPorFiltroPaginadoAsync(filtro);

                if (resultado.Items == null || !resultado.Items.Any())
                {
                    return new TableModel
                    {
                        Headers = new List<string> { "Código", "Año", "Periodo", "Acciones" },
                        Rows = new List<TableRowModel>(),
                        Pagination = new PaginationInfo
                        {
                            CurrentPage = pagina,
                            PageSize = cantidad,
                            TotalItems = 0,
                            OnPageChange = "cambiarPaginaPeriodos"
                        }
                    };
                }

                return new TableModel
                {
                    Headers = new List<string> { "Código", "Año", "Periodo", "Acciones" },
                    Rows = resultado.Items.Select(a => new TableRowModel
                    {
                        Cells = new List<TableCellModel>
                {
                    new TableCellModel { Value = a.Codigo },
                    new TableCellModel { Value = a.Anio.ToString() },
                    new TableCellModel { Value = a.Periodo },
                    new TableCellModel
                    {
                        Actions = new List<TableActionModel>
                        {
                            new TableActionModel()
                            {
                                Accion = "Editar",
                                OnClick = $"abrirModalEditarPeriodoEscolar({a.IdPeriodoEscolar}, '{a.Anio}', '{a.Periodo}')"
                            },
                            new TableActionModel()
                            {
                                Accion = "Eliminar",
                                OnClick = $"abrirModalConfirmacion('¿Desea eliminar este periodo?', function() {{ eliminarPeriodoEscolar({a.IdPeriodoEscolar}); }})"
                            }
                        }
                    }
                }
                    }).ToList(),
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = pagina,
                        PageSize = cantidad,
                        TotalItems = resultado.TotalCount,
                        OnPageChange = "cambiarPagina"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llenar la tabla de períodos");
                TempData["Error"] = ex.Message;

                return new TableModel();
            }
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
                    TempData["Success"] = $"Periodo Escolar creado exitosamente";
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
                    TempData["Success"] = $"Periodo Escolar editado exitosamente";
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
                {
                    TempData["Success"] = $"Periodo eliminado exitosamente";
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
