using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Commons.Factories;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Docentes;
using SGPla.Models.DTOs.Grados;
using SGPla.Models.ViewModels.Docentes;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers
{
    public class DocentesController : Controller
    {
        private readonly IDocenteService _docenteService;
        private readonly IAspiranteService _aspiranteService;
        private readonly IArchivoService _archivoService;
        private readonly ILogger<DocentesController> _logger;

        private int paginaActual = 1;

        private const string NOMBRE_LOGGER = "FRONT-DOCENTES-";
        private static List<string> HEADERS_TABLA_DOCENTE = ["Nombre", "Número personal", "Puesto", "Acciones"];
        private static List<string> HEADERS_TABLA_ASPIRANTE = ["Nombre", "Grado", "Perfil", "Acciones"];
        private static List<string> HEADERS_TABLA_GRADOS = ["Grado", "Área", "Mayor grado de estudios", "Acciones"];
        private const string SESSION_GRADOS_AGREGADOS = "GradosAgregados";

        public DocentesController(
            IDocenteService docenteService,
            IAspiranteService aspiranteService,
            IArchivoService archivoService,
            ILogger<DocentesController> logger)
        {
            _docenteService = docenteService;
            _aspiranteService = aspiranteService;
            _archivoService = archivoService;
            _logger = logger;
        }

        // ==========
        // Index
        // ==========

        //Vista
        [HttpGet]
        public async Task<IActionResult> IndexAsync(string? busqueda, int pagina = 1, int cantidad = 10)
        {
            var modelo = new IndexViewModel();
            try
            {
                var tabDocente = new TabIndexViewModel
                {
                    Tabla = await generarTablaDocentesAsync(busqueda, pagina, cantidad),
                    AccionBoton = Url.Action("Docentes", "RegistrarPersonalDocente")
                };
                var tabAspirante = new TabIndexViewModel
                {
                    Tabla = await generarTablaAspirantesAsync(busqueda, pagina, cantidad),
                    AccionBoton = Url.Action("Docentes", "RegistrarPersonalExterno")
                };

                modelo.TabAspirantes = tabAspirante;
                modelo.TabDocentes = tabDocente;

                return View(modelo);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, "Index:", Constantes.LOG_ERROR_INESPERADO);
                modelo.TabAspirantes.Tabla = TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_DOCENTE, string.Format(Constantes.ERROR_TABLA, Constantes.PERSONALES_EXTERNOS));
                modelo.TabDocentes.Tabla = TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_DOCENTE, string.Format(Constantes.ERROR_TABLA, Constantes.PERSONALES_ACADEMICOS));
                return View(modelo);
            }
        }

        private async Task<TableModel> generarTablaDocentesAsync(string? busqueda, int pagina, int cantidad)
        {
            try
            {
                var datos = await _docenteService.ObtenerTodosDocentesAsync(busqueda, pagina, cantidad);
                var items = datos.items;
                var total = datos.total;

                if (items.Count == 0)
                {
                    datos = await _docenteService.ObtenerTodosDocentesAsync(busqueda, paginaActual, cantidad);
                    items = datos.items;
                    total = datos.total;
                }
                if (items.Count == 0)
                    return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_DOCENTE, string.Format(Constantes.TABLA_VACIA, Constantes.PERSONALES_ACADEMICOS));

                return new TableModel()
                {
                    Headers = HEADERS_TABLA_DOCENTE,
                    Rows = items.Select(docente => new TableRowModel
                    {
                        Cells = new List<TableCellModel>
                        {
                            new() { Value = docente.Nombre },
                            new() { Value = docente.NumeroPersonal },
                            new() { Value = docente.Puesto },
                            new()
                            {
                                Actions = new List<TableActionModel>
                                {
                                    new()
                                    {
                                        Accion = "ver", //Cambiar por documentos
                                        Url = Url.Action("Docentes", "VerDocumentos")
                                    },
                                    new()
                                    {
                                        Accion = "editar",
                                        Url = Url.Action("Docentes", "EditarDocente")
                                    },
                                    new()
                                    {
                                        Accion = "eliminar",
                                        Url = Url.Action("Docentes", "EliminarDocente")
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
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, "Index:", "No se pudo cargar la lista de Docente");
                return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_DOCENTE, string.Format(Constantes.ERROR_TABLA, Constantes.PERSONALES_ACADEMICOS));
            }
        }

        private async Task<TableModel> generarTablaAspirantesAsync(string? busqueda, int pagina, int cantidad)
        {
            try
            {
                var datos = await _aspiranteService.ObtenerTodosAspirantesAsync(busqueda, pagina, cantidad);
                var items = datos.items;
                var total = datos.total;

                if (items.Count == 0)
                {
                    datos = await _aspiranteService.ObtenerTodosAspirantesAsync(busqueda, pagina, cantidad);
                    items = datos.items;
                    total = datos.total;
                }
                if (items.Count == 0)
                    return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_ASPIRANTE, string.Format(Constantes.TABLA_VACIA, Constantes.PERSONALES_EXTERNOS));

                return new TableModel()
                {
                    Headers = HEADERS_TABLA_DOCENTE,
                    Rows = items.Select(aspirante => new TableRowModel
                    {
                        Cells = new List<TableCellModel>
                        {
                            new() { Value = aspirante.Nombre },
                            new() { Value = aspirante.UltimoGrado },
                            new()
                            {
                                Actions = new List<TableActionModel>
                                    {
                                        new()
                                        {
                                            Accion = "informacion",
                                            OnClick = $"abrirModalPerfil('{aspirante.DescripcionPerfil}')"
                                        }
                                    }
                            },
                            new()
                            {
                                Actions = new List<TableActionModel>
                                {
                                    new()
                                    {
                                        Accion = "ver", //Cambiar por documentos
                                        Url = Url.Action("Docentes", "VerDocumentos")
                                    },
                                    new()
                                    {
                                        Accion = "editar",
                                        Url = Url.Action("Docentes", "EditarAspirante")
                                    },
                                    new()
                                    {
                                        Accion = "eliminar",
                                        Url = Url.Action("Docentes", "EliminarAspirante")
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
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, "Index:", "No se pudo cargar la lista de Aspirante");
                return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_ASPIRANTE, string.Format(Constantes.ERROR_TABLA, Constantes.PERSONALES_EXTERNOS));
            }
        }

        // ====================
        // RegistrarDocente
        // ====================

        //Vista
        [HttpGet]
        public async Task<IActionResult> RegistrarPersonalAcademicoAsync(RegistrarDocenteViewModel modelo)
        {
            var vista = new RegistrarDocenteViewModel();
            var error = false;

            if (modelo.Recarga)
            {
                var datos = recargargarRegistroDocente(modelo);
                vista = datos.vista;
                error = datos.error;
            }
            else
                return View("RegistrarPersonalDocenteAsync", inicializarRegistroDocente());

            if (error)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, "RegistrarPersonalAcademicoAsync:", "No se pudieron cargar los grados agregados.");
                return View("IndexAsync");
            }

            return View("RegistrarPersonalAcademicoAsync", vista);
        }

        private RegistrarDocenteViewModel inicializarRegistroDocente()
        {
            var modelo = new RegistrarDocenteViewModel()
            {
                Tabla = TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_GRADOS, "No se han agregado grados."),
                OpcionesPuesto = generarListaPuestos(""),
                Recarga = true
            };

            HttpContext.Session.SetString(SESSION_GRADOS_AGREGADOS, string.Empty);

            return modelo;
        }

        private (RegistrarDocenteViewModel vista, bool error) recargargarRegistroDocente(RegistrarDocenteViewModel modelo)
        {
            var gradosAgregados = HttpContext.Session.GetString(SESSION_GRADOS_AGREGADOS);
            if (!string.IsNullOrEmpty(gradosAgregados))
            {
                var listaGrados = System.Text.Json.JsonSerializer.Deserialize<List<AgregarGradoDTO>>(gradosAgregados);
                modelo.OpcionesPuesto = generarListaPuestos(modelo.Puesto);
                modelo.Tabla = generarTablaGradosRegistro(listaGrados);

                return (modelo, false);
            }
            else
                return (modelo, true);
        }

        private List<OptionModel> generarListaPuestos(string puesto)
        {
            var listaPuestos = new List<OptionModel>();
            foreach (var p in Constantes.PUESTOS)
            {
                if (p == puesto)
                    listaPuestos.Add(new OptionModel { Value = p, Text = p, Selected = true });
                else
                    listaPuestos.Add(new OptionModel { Value = p, Text = p });
            }
            return listaPuestos;
        }

        [HttpPost]
        public async Task GuardarDocenteAsync(RegistrarDocenteViewModel modelo)
        {
            //TODO: Implementar método para guardar docente
        }

        private TableModel generarTablaGradosRegistro(List<AgregarGradoDTO> lista)
        {
            if (lista.Count == 0)
                return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_GRADOS, "No se han agregado grados.");
            return new TableModel()
            {
                Headers = HEADERS_TABLA_GRADOS,
                Rows = lista.Select(grado => new TableRowModel
                {
                    Cells = new List<TableCellModel>
                    {
                        new() { Value = grado.Grado },
                        new() { Value = grado.Titulo },
                        new()
                        {
                            CheckBox = grado.Ultimo
                        },
                        new()
                        {
                            Actions = new List<TableActionModel>
                            {
                                new()
                                {
                                    Accion = "editar",
                                    OnClick = $"editarGrado('{grado.IdTemporal}')"
                                },
                                new()
                                {
                                    Accion = "eliminar",
                                    OnClick = $"eliminarGrado('{grado.IdTemporal}')"
                                }
                            }
                        }
                    }
                }).ToList()
            };
        }
    }
}
