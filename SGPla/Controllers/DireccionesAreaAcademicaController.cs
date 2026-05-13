using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SGPla.Models.DTOs.AreaAcademica;
using SGPla.Models.DTOs.Articulo;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers
{
    public class DireccionesAreaAcademicaController: Controller
    {
        private readonly IAreaAcademicaService _areaAcademicaService;
        private readonly ILogger<DireccionesAreaAcademicaController> _logger;
        private int paginaActual = 1;

        public DireccionesAreaAcademicaController(IAreaAcademicaService areaAcademicaService, ILogger<DireccionesAreaAcademicaController> logger)
        {
            _areaAcademicaService = areaAcademicaService;
            _logger = logger;
        }

        //GET: DireccionesAreaAcademica

        public async Task<IActionResult> Index(string? busqueda, int pagina = 1, int cantidad = 10)
        {
            paginaActual = pagina;
                return View(new Models.ViewModels.DireccionesAreaAcademica.IndexViewModel
                {
                    Table = await LlenarTabla(busqueda, pagina, cantidad),
                    Busqueda = busqueda,
                    PaginaActual = paginaActual,
                    CantidadPorPagina = cantidad
                });
        }
        private async Task<TableModel> LlenarTabla(string? busqueda, int pagina = 1, int cantidad = 10)
        {
            try
            {
                var areas = await _areaAcademicaService.BuscarPorFiltroPaginadoAsync(busqueda, pagina, cantidad);
                
                if (areas.Items.Count == 0)
                {
                    paginaActual = 1;
                    areas = await _areaAcademicaService.BuscarPorFiltroPaginadoAsync(busqueda, paginaActual, cantidad);
                }

                return new TableModel
                {
                    Headers = new List<string> { "Nombre de la Dirección", "Domicillio", "Teléfono", "Acciones" },
                    Rows = areas.Items.Select(a => new TableRowModel
                    {
                        Cells = new List<TableCellModel>
                    {
                        new TableCellModel { Value = a.Nombre},
                        new TableCellModel { Value = a.Domicilio},
                        new TableCellModel { Value = a.Telefono},
                        new TableCellModel
                        {
                            Actions = new List<TableActionModel>
                            {
                                new()
                                {
                                    Accion = "ver",
                                    Url = Url.Action("VerDireccionAreaAcademica", "DireccionesAreaAcademica", new { id = a.IdAreaAcademica })
                                },
                                new()
                                {
                                    Accion = "editar",
                                    Url = Url.Action("EditarDireccionAreaAcademica", "DireccionesAreaAcademica", new { id = a.IdAreaAcademica })
                                },
                                new()
                                {
                                    Accion = "eliminar",
                                    OnClick = $"abrirModalConfirmacion('¿Desea eliminar esta dirección de área académica?', function() {{ eliminarDireccionAreaAcademica({a.IdAreaAcademica}); }})"
                                }
                            }
                        }
                    },

                    }).ToList(),
                    Pagination = new PaginationInfo
                    {
                        TotalItems = areas.TotalCount,
                        PageSize = cantidad,
                        CurrentPage = paginaActual,
                        OnPageChange = "cambiarPagina"
                    }
                };
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llenar la tabla de artículos");
                TempData["Error"] = ex.Message;

                return new TableModel();
            }
        }


        // Crear

        public async Task<IActionResult> CrearDireccionAreaAcademica(int? idDireccion)
        {
            return View(new CrearAreaAcademicaDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearDireccionAreaAcademica(CrearAreaAcademicaDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _areaAcademicaService.CrearAsync(model);
                TempData["Success"] = "Dirección de área académica creada correctamente";
                return RedirectToAction("Index");
            }
            catch(ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error al crear la dirección de área académica");
                TempData["Error"] = ex.Message;
                return View(model);
            }

        }

        // Ver

        public async Task<IActionResult> VerDireccionAreaAcademica(int id)
        {
            try
            {
                if (id <= 0 || string.IsNullOrEmpty(id.ToString()))
                {
                    return BadRequest();
                }

                var area = await _areaAcademicaService.ObtenerPorIdAsync(id);
                if (area == null)
                {
                    return NotFound();
                }
                return View(area);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener la dirección de área académica con ID {id}");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        // Editar

        public async Task<IActionResult> EditarDireccionAreaAcademica(int id)
        {
            var direccion = await _areaAcademicaService.ObtenerPorIdAsync(id);



            return View(direccion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarDireccionAreaAcademica(DatosAreaAcademicaDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try
            {
                await _areaAcademicaService.EditarAsync(model);
                TempData["Success"] = "Dirección editada correctamente";
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

        // Eliminar

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarDireccionAreaAcademica(int id)
        {
            try
            {
                await _areaAcademicaService.EliminarAsync(id);
                TempData["Success"] = "Dirección de área académica eliminada correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Direccion de área académica no encontrada: {Id}", id);
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la dirección de área académica {Id}", id);
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
