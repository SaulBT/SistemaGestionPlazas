using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Commons.Factories;
using SGPla.Models.Components;
using SGPla.Models.DTOs.IntegranteCt;
using SGPla.Models.ViewModels.IntegranteCt;
using SGPla.Services.Interfaces;
using static Azure.Core.HttpHeader;

namespace SGPla.Controllers
{
    public class IntegranteCtController : Controller
    {
        private readonly IIntegranteCtService _integranteCtService;
        private readonly ILogger<IntegranteCtController> _logger;
        private int paginaActual = 1;

        private static List<string> HEADERS_TABLA = ["Cargo", "Nombres", "Acciones"];
        private const string SESSION_ID_ENTIDAD = "IdEntidadAcademica";
        private const string NOMBRE_LOGGER = "FRONT-INTEGRANTES-";
        private const string LOG_ERROR_ID = "La IdEntidadAcademica no es válida";

        public IntegranteCtController(IIntegranteCtService integranteCtService, ILogger<IntegranteCtController> logger)
        {
            _integranteCtService = integranteCtService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? nombre, int pagina = 1, int cantidad = 10)
        {
            HttpContext.Session.SetInt32(SESSION_ID_ENTIDAD, 1);
            var vista = new IndexViewModel
            {
                Table = TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA, string.Format(Constantes.ERROR_TABLA, Constantes.INTEGRANTES_CT)),
                PaginaActual = paginaActual,
                CantidadPorPagina = cantidad
            };
            paginaActual = pagina;
            vista.Formulario = new FormularioIntegranteViewModel();
            var gradosCombo = Constantes.GRADOS
                .Select(g => new OptionModel
                {
                    Value = g,
                    Text = g,
                    Selected = false
                }).ToList();

            vista.Formulario.Grados = gradosCombo;

            try
            {
                int? idEntidadAcademica = HttpContext.Session.GetInt32(SESSION_ID_ENTIDAD);
                if (idEntidadAcademica != null && idEntidadAcademica > 0)
                {
                    vista.Table = await LlenarTablaAsync(nombre, (int)idEntidadAcademica, pagina, cantidad);
                }
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, "Index:", LOG_ERROR_ID);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, "Index:", Constantes.LOG_ERROR_INESPERADO);
            }
            return View(vista);
        }

        private async Task<TableModel> LlenarTablaAsync(string? nombre, int idEntidadAcademica, int pagina = 1, int cantidad = 10)
        {
            try
            {
                var busqueda = new BusquedaIntegranteCtDto
                {
                    Cantidad = cantidad,
                    IdEntidadAcademica = idEntidadAcademica,
                    Nombre = nombre ?? "",
                    Pagina = pagina
                };
                var datos = await _integranteCtService.ObtenerTodosIntegrantesPorPaginaAsync(busqueda);
                if (datos.items.Count == 0)
                {
                    paginaActual = 1;
                    busqueda.Pagina = paginaActual;
                    datos = await _integranteCtService.ObtenerTodosIntegrantesPorPaginaAsync(busqueda);
                }
                if (datos.items.Count == 0)
                    return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA, string.Format(Constantes.TABLA_VACIA, Constantes.INTEGRANTES_CT));

                var integrantes = datos.items;
                var total = datos.cantidad;

                return new TableModel
                {
                    Headers = HEADERS_TABLA,
                    Rows = integrantes.Select(integrante => new TableRowModel
                    {
                        Cells = new List<TableCellModel>
                        {
                            new() {Value = integrante.Cargo},
                            new() {Value = integrante.Grado + " " + integrante.Nombre},
                            new()
                            {
                                Actions = new List<TableActionModel>
                                {
                                    new()
                                    {
                                        Accion = "editar",
                                        OnClick = $"abrirModalEditarIntegrante({integrante.IdIntegranteCt}, '{integrante.Cargo}', '{integrante.Nombre}', '{integrante.Grado}')"
                                    },
                                    new()
                                    {
                                        Accion = "eliminar",
                                        OnClick = $"abrirModalConfirmacion('¿Desea eliminar este integrante?', function() {{ eliminarIntegrante({integrante.IdIntegranteCt}); }})"
                                    }
                                }
                            }
                        }
                    }).ToList(),
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = paginaActual,
                        PageSize = cantidad,
                        TotalItems = total,
                        OnPageChange = "cambiarPagina"
                    }
                };
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, "Index:", "No se pudo cargar la lista de Integrantes de Consejo Técnico");
                return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA, string.Format(Constantes.ERROR_TABLA, Constantes.INTEGRANTES_CT));

            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(RegistrarIntegranteCtDto dto)
        {
            if (!ModelState.IsValid)
            {
                var gradosCombo = Constantes.GRADOS
                .Select(g => new OptionModel
                {
                    Value = g,
                    Text = g,
                    Selected = false
                }).ToList();

                var model = new IndexViewModel
                {
                    Table = await LlenarTablaAsync("", 1),
                    Formulario = new FormularioIntegranteViewModel
                    {
                        Nombre = dto.Nombre,
                        Cargo = dto.Cargo,
                        Grado = dto.Grado,
                        Grados = gradosCombo
                    }
                };
                return View("Index", model);
            }
            try
            {
                dto.IdEntidadAcademica = (int)HttpContext.Session.GetInt32(SESSION_ID_ENTIDAD);
                await _integranteCtService.RegistrarIntegranteAsync(dto);
                TempData["Success"] = "Integrante registrado exitosamente";
            }
            catch(Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(DatosIntegranteCtDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    dto.IdEntidadAcademica = (int)HttpContext.Session.GetInt32(SESSION_ID_ENTIDAD);
                    await _integranteCtService.EditarIntegranteAsync(dto);
                    TempData["Success"] = "Cambios guardados con éxito";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }
            else
            {
                var gradosCombo = Constantes.GRADOS
                .Select(g => new OptionModel
                {
                    Value = g,
                    Text = g,
                    Selected = false
                }).ToList();

                var model = new IndexViewModel
                {
                    Table = await LlenarTablaAsync("", 1),
                    Formulario = new FormularioIntegranteViewModel
                    {
                        IdIntegranteCt = dto.IdIntegranteCt,
                        Nombre = dto.Nombre,
                        Cargo = dto.Cargo,
                        Grado = dto.Grado,
                        Grados = gradosCombo
                    }
                };
                return View("Index", model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                await _integranteCtService.EliminarIntegranteAsync(id);
                TempData["Success"] = "Integrante eliminado exitosamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Buscar(string busqueda)
        {
            try
            {
                BusquedaIntegranteCtDto dto = new BusquedaIntegranteCtDto
                {
                    IdEntidadAcademica = (int)HttpContext.Session.GetInt32(SESSION_ID_ENTIDAD),
                    Nombre = busqueda,
                    Pagina = paginaActual
                };
                var resultados = await _integranteCtService.ObtenerTodosIntegrantesPorPaginaAsync(dto);
                return View("Index", resultados);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }


    }
}
