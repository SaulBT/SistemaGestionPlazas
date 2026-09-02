using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SGPla.Commons;
using SGPla.Commons.Factories;
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
        private int paginaActual = 1;

        private static List<string> HEADERS_TABLA_INDEX = ["Código", "Año de ejercicio", "Periodo", "Acciones"];

        public PeriodosEscolaresController(IPeriodoEscolarService periodoEscolarService, ILogger<PeriodosEscolaresController> logger)
        {
            _periodoEscolarService = periodoEscolarService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? anioFiltro, string? periodoFiltro, int pagina = 1, int cantidad = 10)
        {
            

            paginaActual = pagina;
            return View(new IndexViewModel
            {
                Table = await LlenarTabla(anioFiltro, periodoFiltro, pagina, cantidad),
                AnioFiltro = anioFiltro,
                Periodo = periodoFiltro,
                PaginaActual = paginaActual,
                CantidadPorPagina = cantidad
            });
        }


        private async Task<TableModel> LlenarTabla(string? anioFiltro, string? periodoFiltro, int pagina = 1, int cantidad = 10)
        {
            try
            {
                var filtro = new BuscarPeriodoEscolarDTO
                {
                    Anio = anioFiltro,
                    Periodo = periodoFiltro,
                    Pagina = pagina,
                    Cantidad = cantidad
                };

                var resultado = await _periodoEscolarService.BuscarPorFiltroPaginadoAsync(filtro);
                if (resultado.Items.Count == 0)
                {
                    paginaActual = 1;
                    filtro.Pagina = paginaActual;
                    resultado = await _periodoEscolarService.BuscarPorFiltroPaginadoAsync(filtro);
                }

                if (resultado.Items == null || !resultado.Items.Any())
                {
                    return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_INDEX, string.Format(Constantes.TABLA_VACIA, Constantes.PERIODOS_ESCOLARES));
                }

                return new TableModel
                {
                    Headers = new List<string> { "Código", "Año de ejercicio", "Periodo", "Acciones" },
                    Rows = resultado.Items.Select(a => new TableRowModel
                    {
                        Cells = new List<TableCellModel>
                {
                    new TableCellModel { Value = a.Codigo },
                    new TableCellModel { Value = a.Anio.ToString() },
                    new TableCellModel { Value = a.PeriodoMostrar },
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
                        CurrentPage = paginaActual,
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

                return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_INDEX, ex.Message);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(PeriodoEscolarFormularioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var indexModel = new IndexViewModel
                {
                    Table = await LlenarTabla(null, null),
                    Formulario = model
                };
                ViewData["AbrirModalCrear"] = true;
                return View("Index", indexModel);
            }

            try
            {
                var dto = new CrearPeriodoEscolarDTO
                {
                    Anio = model.Anio,
                    Periodo = model.Periodo
                };

                await _periodoEscolarService.CrearAsync(dto);

                TempData["Success"] = "Periodo Escolar creado exitosamente";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el periodo escolar");
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }


       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarPeriodoEscolarDTO dto)
        {

            try
            {

                var resultado = await _periodoEscolarService.EditarAsync(dto);
                TempData["Success"] = $"Periodo Escolar editado exitosamente";
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error al crear el programa educativo");
                TempData["Error"] = ex.Message;
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
