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
                Table = TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA, string.Format(Constantes.ERROR_TABLA, Constantes.INTEGRANTES_CT))
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
                            new() {Value = integrante.Nombre},
                            new()
                            {
                                Actions = new List<TableActionModel>
                                {
                                    new()
                                    {
                                        Accion = "editar",
                                    },
                                    new()
                                    {
                                        Accion = "eliminar",
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
    }
}
