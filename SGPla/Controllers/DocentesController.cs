using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Commons.Factories;
using SGPla.Models;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Docentes;
using SGPla.Models.DTOs.Grados;
using SGPla.Models.ViewModels.Docentes;
using SGPla.Services.Interfaces;
using System.Text.Json;

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
        private const string INDEX = "Index:";
        private const string REGISTRAR_ACADEMICO = "RegistrarPersonalAcademicoAsync:";
        private const string REGISTRAR_EXTERNO = "RegistrarPersonalExterno:";
        private const string EDITAR_ACADEMICO = "EditarPersonalAcademicoAsync:";
        private const string EDITAR_EXTERNO = "EditarPersonalExternoAsync:";
        private const string VER_DOCUMENTOS = "VerDocumentos:";

        private const string LOG_ERROR_GRADOS_AGREGADOS = "No se pudo cargar la lista de Grados agregados.";
        private const string LOG_ERROR_GRADOS_EDITADOS = "No se pudo cargar la lista de Grados editados.";
        private const string LOG_ERROR_GRADOS_ELIMINADOS = "No se pudo cargar la lista de Grados eliminados";
        private const string LOG_ERROR_GRADOS = "No se pudo cargar la lista de Grados";

        private static List<string> HEADERS_TABLA_DOCENTE = ["Nombre", "Número personal", "Puesto", "Acciones"];
        private static List<string> HEADERS_TABLA_ASPIRANTE = ["Nombre", "Grado", "Perfil", "Acciones"];
        private static List<string> HEADERS_TABLA_GRADOS = ["Grado", "Área", "Mayor grado de estudios", "Acciones"];

        private const string SESSION_GRADOS_AGREGADOS = "GradosAgregados";
        private const string SESSION_GRADOS_EDITADOS = "GradosEditados";
        private const string SESSION_GRADOS_ELIMINADOS = "GradosEliminados";
        private const string SESSION_GRADOS = "Grados";

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
                    AccionBoton = Url.Action("RegistrarPersonalAcademico", "Docentes")
                };
                var tabAspirante = new TabIndexViewModel
                {
                    Tabla = await generarTablaAspirantesAsync(busqueda, pagina, cantidad),
                    AccionBoton = Url.Action("RegistrarPersonalExterno" , "Docentes")
                };

                modelo.TabAspirantes = tabAspirante;
                modelo.TabDocentes = tabDocente;

                return View(modelo);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_INESPERADO);
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
                                        Url = Url.Action("VerDocumentosPersonal", "Docentes", new { id = docente.IdDocente, esDocente = true })
                                    },
                                    new()
                                    {
                                        Accion = "editar",
                                        Url = Url.Action("EditarPersonalAcademico", "Docentes", new { docente.IdDocente })
                                    },
                                    new()
                                    {
                                        Accion = "eliminar",
                                        OnClick = $"abrirModalEliminarDocente({docente.IdDocente})"
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
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, INDEX, "No se pudo cargar la lista de Docente");
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
                                        Url = Url.Action("VerDocumentosPersonal", "Docentes", new { id = aspirante.IdDocente, esDocente = false })
                                    },
                                    new()
                                    {
                                        Accion = "editar",
                                        Url = Url.Action("EditarPersonalExterno", "Docentes", new { aspirante.IdDocente })
                                    },
                                    new()
                                    {
                                        Accion = "eliminar",
                                        OnClick = $"abrirModalEliminarAspirante({aspirante.IdDocente})"
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
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, INDEX, "No se pudo cargar la lista de Aspirante");
                return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_ASPIRANTE, string.Format(Constantes.ERROR_TABLA, Constantes.PERSONALES_EXTERNOS));
            }
        }

        [HttpGet]
        public async Task EliminarDocenteAsync(int idDocente)
        {
            try
            {
                await _docenteService.EliminarDocenteAsync(idDocente);
                TempData["Success"] = string.Format(Constantes.TOAST_ELIMINACION_EL, Constantes.PERSONAL_ACADEMICO);
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_VALIDACION, string.Format(Constantes.TOAST_ERROR_ELIMINACION_EL, Constantes.PERSONAL_ACADEMICO));
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_INESPERADO, string.Format(Constantes.TOAST_ERROR_ELIMINACION_EL, Constantes.PERSONAL_ACADEMICO));
            }
        }

        [HttpGet]
        public async Task EliminarAspiranteAsync(int idDocente)
        {
            try
            {
                await _aspiranteService.EliminarAspiranteAsync(idDocente);
                TempData["Success"] = string.Format(Constantes.TOAST_ELIMINACION_EL, Constantes.PERSONAL_EXTERNO);
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_VALIDACION, string.Format(Constantes.TOAST_ERROR_ELIMINACION_EL, Constantes.PERSONAL_EXTERNO));
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_INESPERADO, string.Format(Constantes.TOAST_ERROR_ELIMINACION_EL, Constantes.PERSONAL_EXTERNO));
            }
        }

        // ====================
        // RegistrarDocente
        // ====================

        //Vista
        [HttpGet]
        public IActionResult RegistrarPersonalAcademico(RegistrarDocenteViewModel modelo)
        {
            try
            {
                var vista = new RegistrarDocenteViewModel();
                var error = false;

                if (modelo.Recarga)
                {
                    var datos = recargargarRegistroDocente(modelo);
                    vista = datos.vista;
                    error = datos.error;

                    return View("RegistrarPersonalAcademico", vista);
                }

                if (error)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, REGISTRAR_ACADEMICO, LOG_ERROR_GRADOS_AGREGADOS);
                    return View("Index");
                }

                ModelState.Clear();
                return View("RegistrarPersonalAcademico", inicializarRegistroDocente());
            }
            catch (JsonException jx)
            {
                this.LanzarError(_logger, jx, NOMBRE_LOGGER, REGISTRAR_ACADEMICO, Constantes.LOG_ERROR_JSON);
                return RedirectToAction("Index");
            }
        }

        private RegistrarDocenteViewModel inicializarRegistroDocente()
        {
            var modelo = new RegistrarDocenteViewModel()
            {
                Formulario = new FormularioGradoViewModel
                {
                    ListaGrados = generarListaGrados()
                },
                Tabla = TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_GRADOS, "No se han agregado grados."),
                OpcionesPuesto = generarListaPuestos(""),
                Recarga = true
            };

            guardarEnSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS, new List<AgregarGradoDTO>());
            

            return modelo;
        }

        private (RegistrarDocenteViewModel vista, bool error) recargargarRegistroDocente(RegistrarDocenteViewModel modelo)
        {
            var grados = obtenerDeSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS);
            if (grados is null)
                return (modelo, true);

            modelo.OpcionesPuesto = generarListaPuestos(modelo.Puesto);
            modelo.Tabla = generarTablaGradosRegistro(grados);
            modelo.Formulario.ListaGrados = generarListaGrados();

            return (modelo, false);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarDocenteAsync(RegistrarDocenteViewModel modelo)
        {
            try
            {
                var grados = obtenerDeSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS);
                if (grados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, REGISTRAR_ACADEMICO, LOG_ERROR_GRADOS_AGREGADOS);
                    return RedirectToAction("Index");
                }

                if (!validarRegistroDocente(grados, modelo))
                    return RegistrarPersonalAcademico(modelo);

                await procesarGuardadoDocenteAsync(modelo, grados);
                TempData["Success"] = string.Format(Constantes.TOAST_GUARDADO_EL, Constantes.PERSONAL_ACADEMICO);
                return RedirectToAction("Index");
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, REGISTRAR_ACADEMICO, Constantes.LOG_ERROR_VALIDACION, vx.Message);
                return RegistrarPersonalAcademico(modelo);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, REGISTRAR_ACADEMICO, Constantes.LOG_ERROR_INESPERADO);
                return RegistrarPersonalAcademico(modelo);
            }
        }

        private bool validarRegistroDocente(List<AgregarGradoDTO> grados, RegistrarDocenteViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                if (modelo.Archivo == null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, REGISTRAR_ACADEMICO, Constantes.LOG_ERROR_VALIDACION, "Debes cargar un archivo.");
                    return false;
                }

                return false;
            }


            if (grados.Count < 1)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, REGISTRAR_ACADEMICO, Constantes.LOG_ERROR_VALIDACION, "Agrega mínimo un Grado.");
                return false;
            }

            var hayUlimo = false;
            var cantidadUltimo = 0;
            foreach (var grado in grados)
            {
                if (grado.Ultimo)
                {
                    hayUlimo = true;
                    cantidadUltimo++;
                }
            }
            if (!hayUlimo)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, REGISTRAR_ACADEMICO, Constantes.LOG_ERROR_VALIDACION, "Debe haber un Grado marcado como último.");
                return false;
            }
            if (cantidadUltimo > 1)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, REGISTRAR_ACADEMICO, Constantes.LOG_ERROR_VALIDACION, "Sólo puede haber un Grado marcado como último.");
                return false;
            }

            return true;
        }

        private async Task procesarGuardadoDocenteAsync(RegistrarDocenteViewModel modelo, List<AgregarGradoDTO> grados)
        {
            var (nombre, ruta) = await _archivoService.GuardarTemporalmenteAsync(modelo.Archivo);

            var dto = new RegistrarDocenteDTO
            {
                Nombre = modelo.Nombre,
                DescripcionPerfil = modelo.DescripcionPerfil,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = nombre,
                    RutaArchivo = ruta
                },
                Grados = grados,
                NumeroPersonal = modelo.NumeroPersonal,
                Puesto = modelo.Puesto
            };
            await _docenteService.RegistrarDocenteAsync(dto);
            System.IO.File.Delete(ruta);
        }

        // ====================
        // RegistrarAspirante
        // ====================

        //Vista
        [HttpGet]
        public IActionResult RegistrarPersonalExterno(RegistrarDocenteViewModel modelo)
        {
            try
            {
                var vista = new RegistrarDocenteViewModel();
                var error = false;

                if (modelo.Recarga)
                {
                    var datos = recargargarRegistroAspirante(modelo);
                    vista = datos.vista;
                    error = datos.error;

                    return View("RegistrarPersonalExterno", vista);
                }

                if (error)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, REGISTRAR_EXTERNO, LOG_ERROR_GRADOS_AGREGADOS);
                    return View("Index");
                }

                ModelState.Clear();
                return View("RegistrarPersonalExterno", inicializarRegistroAspirante());
            }
            catch (JsonException jx)
            {
                this.LanzarError(_logger, jx, NOMBRE_LOGGER, REGISTRAR_EXTERNO, Constantes.LOG_ERROR_JSON);
                return RedirectToAction("Index");
            }
        }

        private RegistrarDocenteViewModel inicializarRegistroAspirante()
        {
            var modelo = new RegistrarDocenteViewModel()
            {
                Formulario = new FormularioGradoViewModel
                {
                    ListaGrados = generarListaGrados()
                },
                Tabla = TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_GRADOS, "No se han agregado grados."),
                Recarga = true
            };

            guardarEnSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS, new List<AgregarGradoDTO>());


            return modelo;
        }

        private (RegistrarDocenteViewModel vista, bool error) recargargarRegistroAspirante(RegistrarDocenteViewModel modelo)
        {
            var grados = obtenerDeSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS);
            if (grados is null)
                return (modelo, true);

            modelo.Tabla = generarTablaGradosRegistro(grados);
            modelo.Formulario.ListaGrados = generarListaGrados();

            return (modelo, false);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarAspiranteAsync(RegistrarDocenteViewModel modelo)
        {
            try
            {
                var grados = obtenerDeSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS);
                if (grados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, REGISTRAR_EXTERNO, LOG_ERROR_GRADOS_AGREGADOS);
                    return RedirectToAction("Index");
                }

                if (!validarRegistroAspirante(grados, modelo))
                    return RegistrarPersonalExterno(modelo);

                await procesarGuardadoAspiranteAsync(modelo, grados);
                TempData["Success"] = string.Format(Constantes.TOAST_GUARDADO_EL, Constantes.PERSONAL_EXTERNO);
                return RedirectToAction("Index");
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, REGISTRAR_EXTERNO, Constantes.LOG_ERROR_VALIDACION, vx.Message);
                return RegistrarPersonalExterno(modelo);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, REGISTRAR_EXTERNO, Constantes.LOG_ERROR_INESPERADO);
                return RegistrarPersonalExterno(modelo);
            }
        }

        private bool validarRegistroAspirante(List<AgregarGradoDTO> grados, RegistrarDocenteViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                if (modelo.Archivo == null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, REGISTRAR_EXTERNO, Constantes.LOG_ERROR_VALIDACION, "Debes cargar un archivo.");
                    return false;
                }
                if (string.IsNullOrEmpty(modelo.Nombre) ||
                    string.IsNullOrEmpty(modelo.DescripcionPerfil)
                   )
                    return false;
            }

            if (grados.Count < 1)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, REGISTRAR_EXTERNO, Constantes.LOG_ERROR_VALIDACION, "Agrega mínimo un Grado.");
                return false;
            }

            var hayUlimo = false;
            var cantidadUltimo = 0;
            foreach (var grado in grados)
            {
                if (grado.Ultimo)
                {
                    hayUlimo = true;
                    cantidadUltimo++;
                }
            }
            if (!hayUlimo)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, REGISTRAR_EXTERNO, Constantes.LOG_ERROR_VALIDACION, "Debe haber un Grado marcado como último.");
                return false;
            }
            if (cantidadUltimo > 1)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, REGISTRAR_ACADEMICO, Constantes.LOG_ERROR_VALIDACION, "Sólo puede haber un Grado marcado como último.");
                return false;
            }

            return true;
        }

        private async Task procesarGuardadoAspiranteAsync(RegistrarDocenteViewModel modelo, List<AgregarGradoDTO> grados)
        {
            var (nombre, ruta) = await _archivoService.GuardarTemporalmenteAsync(modelo.Archivo);

            var dto = new RegistrarDocenteDTO
            {
                Nombre = modelo.Nombre,
                DescripcionPerfil = modelo.DescripcionPerfil,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = nombre,
                    RutaArchivo = ruta
                },
                Grados = grados
            };
            await _aspiranteService.RegistrarAspiranteAsync(dto);
            System.IO.File.Delete(ruta);
        }

        // =============
        // EditarDocente
        // =============

        //Vista
        [HttpGet]
        public async Task<IActionResult> EditarPersonalAcademicoAsync(EditarDocenteViewModel modelo)
        {
            if (modelo.Recarga)
            {
                var (vista, error) = recargarEdicionDocente(modelo);
                if (!error)
                    return View("EditarPersonalAcademico", vista);
                else
                    return View("Index");
            }
            else
            {
                ModelState.Clear();
                var (vista, error) = await inicializarEdicionDocenteAsync(modelo.IdDocente);
                if (!error)
                    return View("EditarPersonalAcademico", vista);
                else
                    return View("Index");
            }
        }

        private async Task<(EditarDocenteViewModel modelo, bool error)> inicializarEdicionDocenteAsync(int idDocente)
        {
            var modelo = new EditarDocenteViewModel
            {
                Tabla = TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_GRADOS, string.Format(Constantes.ERROR_TABLA, Constantes.GRADOS)),
                OpcionesPuesto = generarListaPuestos("")
            };

            try
            {
                var datosDocente = await _docenteService.ObtenerDocenteAsync(idDocente);
                modelo.IdDocente = idDocente;
                modelo.Nombre = datosDocente.Nombre;
                modelo.DescripcionPerfil = datosDocente.DescripcionPerfil;
                modelo.Tabla = generarTablaGradosEdicion(datosDocente.Grados);
                modelo.NumeroPersonal = datosDocente.NumeroPersonal;
                modelo.OpcionesPuesto = generarListaPuestos(datosDocente.Puesto);
                modelo.Puesto = datosDocente.Puesto;

                var formularioGrados = new FormularioGradoViewModel
                {
                    ListaGrados = generarListaGrados()
                };
                modelo.Formulario = formularioGrados;
                modelo.Recarga = true;
                modelo.NuevoArchivo = false;

                guardarEnSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS, new List<AgregarGradoDTO>());
                guardarEnSession<List<DatosGradoDTO>>(SESSION_GRADOS_EDITADOS, new List<DatosGradoDTO>());
                guardarEnSession<List<int>>(SESSION_GRADOS_ELIMINADOS, new List<int>());
                guardarEnSession<List<DatosGradoDTO>>(SESSION_GRADOS, datosDocente.Grados);

                return (modelo, false);
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_VALIDACION, string.Format(Constantes.TOAST_ERROR_CARGAR_EL, Constantes.PERSONAL_ACADEMICO));
                return (modelo, true);
            }
            catch (JsonException jx)
            {
                this.LanzarError(_logger, jx, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_JSON);
                return (modelo, true);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_INESPERADO, string.Format(Constantes.TOAST_ERROR_CARGAR_EL, Constantes.PERSONAL_ACADEMICO));
                return (modelo, true);
            }
        }

        private (EditarDocenteViewModel modelo, bool error) recargarEdicionDocente(EditarDocenteViewModel modelo)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(modelo);

                var formularioGrados = new FormularioGradoViewModel
                {
                    ListaGrados = generarListaGrados()
                };
                modelo.Tabla = TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_GRADOS, string.Format(Constantes.ERROR_TABLA, Constantes.GRADOS));
                modelo.OpcionesPuesto = generarListaPuestos(modelo.Puesto);
                modelo.Formulario = formularioGrados;

                var grados = obtenerDeSession<List<DatosGradoDTO>>(SESSION_GRADOS);
                if (grados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, LOG_ERROR_GRADOS, string.Format(Constantes.TOAST_ERROR_GUARDAR_EL, Constantes.PERSONAL_ACADEMICO));
                    return (new EditarDocenteViewModel(), true);
                }

                modelo.Tabla = generarTablaGradosEdicion(grados);

                return (modelo, false);
            }
            catch (ArgumentNullException anx)
            {
                this.LanzarError(_logger, anx, NOMBRE_LOGGER, EDITAR_ACADEMICO, string.Format(Constantes.LOG_GENERAL_NULO, anx.ParamName));
                return (new EditarDocenteViewModel(), true);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditarDocenteAsync(EditarDocenteViewModel modelo)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(modelo);

                var grados = obtenerDeSession<List<DatosGradoDTO>>(SESSION_GRADOS);
                if (grados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, LOG_ERROR_GRADOS);
                    return RedirectToAction("Index");
                }
                var gradosAgregados = obtenerDeSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS);
                if (gradosAgregados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, LOG_ERROR_GRADOS_AGREGADOS);
                    return RedirectToAction("Index");
                }
                var gradosEditados = obtenerDeSession<List<DatosGradoDTO>>(SESSION_GRADOS_EDITADOS);
                if (gradosEditados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, LOG_ERROR_GRADOS_EDITADOS);
                    return RedirectToAction("Index");
                }
                var gradosEliminados = obtenerDeSession<List<int>>(SESSION_GRADOS_ELIMINADOS);
                if (gradosEliminados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, LOG_ERROR_GRADOS_ELIMINADOS);
                    return RedirectToAction("Index");
                }

                if (!validarEdicionDocente(grados, modelo))
                    return await EditarPersonalAcademicoAsync(modelo);

                await procesarEdicionDocenteAsync(modelo, gradosAgregados, gradosEditados, gradosEliminados);
                TempData["Success"] = string.Format(Constantes.TOAST_GUARDADO_EL, Constantes.PERSONAL_ACADEMICO);

                return RedirectToAction("Index");
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_JSON, vx.Message);
                return await EditarPersonalAcademicoAsync(modelo);
            }
            catch (JsonException jx)
            {
                this.LanzarError(_logger, jx, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_JSON);
                return RedirectToAction("Index");
            }
            catch (ArgumentNullException anx)
            {
                this.LanzarError(_logger, anx, NOMBRE_LOGGER, EDITAR_ACADEMICO, string.Format(Constantes.LOG_GENERAL_NULO, anx.ParamName));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_INESPERADO);
                return RedirectToAction("Index");
            }
        }

        private bool validarEdicionDocente(List<DatosGradoDTO> grados, EditarDocenteViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                if (
                    string.IsNullOrEmpty(modelo.Nombre) ||
                    string.IsNullOrEmpty(modelo.DescripcionPerfil) ||
                    string.IsNullOrEmpty(modelo.NumeroPersonal) ||
                    string.IsNullOrEmpty(modelo.Puesto)
                   )
                    return false;
            }

            if (grados.Count < 1)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, REGISTRAR_ACADEMICO, Constantes.LOG_ERROR_VALIDACION, "Agrega mínimo un Grado.");
                return false;
            }
            var hayUlimo = false;
            var cantidadUltimo = 0;
            foreach (var grado in grados)
            {
                if (grado.Ultimo)
                {
                    hayUlimo = true;
                    cantidadUltimo++;
                }
            }
            if (!hayUlimo)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_VALIDACION, "Debe haber un Grado marcado como último.");
                return false;
            }
            if (cantidadUltimo > 1)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_VALIDACION, "Sólo puede haber un Grado marcado como último.");
                return false;
            }

            return true;
        }

        private async Task procesarEdicionDocenteAsync(EditarDocenteViewModel modelo, List<AgregarGradoDTO> gradosAgregados, List<DatosGradoDTO> gradosEditados, List<int> gradosEliminados)
        {
            var docenteDto = new EditarDocenteDTO
            {
                IdDocente = modelo.IdDocente,
                Nombre = modelo.Nombre,
                DescripcionPerfil = modelo.DescripcionPerfil,
                GradosAgregados = gradosAgregados,
                GradosEditados = gradosEditados,
                IdsGradosEliminados = gradosEliminados,
                NumeroPersonal = modelo.NumeroPersonal,
                Puesto = modelo.Puesto
            };

            if (modelo.Archivo is not null)
            {
                var (nombre, ruta) = await _archivoService.GuardarTemporalmenteAsync(modelo.Archivo);
                var archivoDTO = new CargarArchivoDTO
                {
                    NombreArchivo = nombre,
                    RutaArchivo = ruta
                };

                docenteDto.ArchivosGenerales = archivoDTO;
                docenteDto.NuevoArchivo = true;

                await _docenteService.EditarDocenteAsync(docenteDto);
                System.IO.File.Delete(ruta);
            }
            else
                await _docenteService.EditarDocenteAsync(docenteDto);
        }

        // ===============
        // EditarAspirante
        // ===============

        //Vista
        [HttpGet]
        public async Task<IActionResult> EditarPersonalExternoAsync(EditarDocenteViewModel modelo)
        {
            if (modelo.Recarga)
            {
                var (vista, error) = recargarEdicionAspirante(modelo);
                if (!error)
                    return View("EditarPersonalExterno", vista);
                else
                    return View("Index");
            }
            else
            {
                ModelState.Clear();
                var (vista, error) = await inicializarEdicionAspiranteAsync(modelo.IdDocente);
                if (!error)
                    return View("EditarPersonalExterno", vista);
                else
                    return View("Index");
            }
        }

        private async Task<(EditarDocenteViewModel modelo, bool error)> inicializarEdicionAspiranteAsync(int idDocente)
        {
            var modelo = new EditarDocenteViewModel
            {
                Tabla = TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_GRADOS, string.Format(Constantes.ERROR_TABLA, Constantes.GRADOS)),
            };

            try
            {
                var datosDocente = await _docenteService.ObtenerDocenteAsync(idDocente);
                modelo.IdDocente = idDocente;
                modelo.Nombre = datosDocente.Nombre;
                modelo.DescripcionPerfil = datosDocente.DescripcionPerfil;
                modelo.Tabla = generarTablaGradosEdicion(datosDocente.Grados);

                var formularioGrados = new FormularioGradoViewModel
                {
                    ListaGrados = generarListaGrados()
                };
                modelo.Formulario = formularioGrados;
                modelo.Recarga = true;
                modelo.NuevoArchivo = false;

                guardarEnSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS, new List<AgregarGradoDTO>());
                guardarEnSession<List<DatosGradoDTO>>(SESSION_GRADOS_EDITADOS, new List<DatosGradoDTO>());
                guardarEnSession<List<int>>(SESSION_GRADOS_ELIMINADOS, new List<int>());
                guardarEnSession<List<DatosGradoDTO>>(SESSION_GRADOS, datosDocente.Grados);

                return (modelo, false);
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, EDITAR_EXTERNO, Constantes.LOG_ERROR_VALIDACION, string.Format(Constantes.TOAST_ERROR_CARGAR_EL, Constantes.PERSONAL_EXTERNO));
                return (modelo, true);
            }
            catch (JsonException jx)
            {
                this.LanzarError(_logger, jx, NOMBRE_LOGGER, EDITAR_EXTERNO, Constantes.LOG_ERROR_JSON, string.Format(Constantes.TOAST_ERROR_CARGAR_EL, Constantes.PERSONAL_EXTERNO));
                return (modelo, true);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, EDITAR_EXTERNO, Constantes.LOG_ERROR_INESPERADO, string.Format(Constantes.TOAST_ERROR_CARGAR_EL, Constantes.PERSONAL_EXTERNO));
                return (modelo, true);
            }
        }

        private (EditarDocenteViewModel modelo, bool error) recargarEdicionAspirante(EditarDocenteViewModel modelo)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(modelo);

                var formularioGrados = new FormularioGradoViewModel
                {
                    ListaGrados = generarListaGrados()
                };
                modelo.Tabla = TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_GRADOS, string.Format(Constantes.ERROR_TABLA, Constantes.GRADOS));
                modelo.Formulario = formularioGrados;

                var grados = obtenerDeSession<List<DatosGradoDTO>>(SESSION_GRADOS);
                if (grados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_EXTERNO, LOG_ERROR_GRADOS, string.Format(Constantes.TOAST_ERROR_GUARDAR_EL, Constantes.PERSONAL_EXTERNO));
                    return (new EditarDocenteViewModel(), true);
                }

                modelo.Tabla = generarTablaGradosEdicion(grados);

                return (modelo, false);
            }
            catch (ArgumentNullException anx)
            {
                this.LanzarError(_logger, anx, NOMBRE_LOGGER, EDITAR_EXTERNO, string.Format(Constantes.LOG_GENERAL_NULO, anx.ParamName));
                return (new EditarDocenteViewModel(), true);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditarAspiranteAsync(EditarDocenteViewModel modelo)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(modelo);

                var grados = obtenerDeSession<List<DatosGradoDTO>>(SESSION_GRADOS);
                if (grados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_EXTERNO, LOG_ERROR_GRADOS);
                    return RedirectToAction("Index");
                }
                var gradosAgregados = obtenerDeSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS);
                if (gradosAgregados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_EXTERNO, LOG_ERROR_GRADOS_AGREGADOS);
                    return RedirectToAction("Index");
                }
                var gradosEditados = obtenerDeSession<List<DatosGradoDTO>>(SESSION_GRADOS_EDITADOS);
                if (gradosEditados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_EXTERNO, LOG_ERROR_GRADOS_EDITADOS);
                    return RedirectToAction("Index");
                }
                var gradosEliminados = obtenerDeSession<List<int>>(SESSION_GRADOS_ELIMINADOS);
                if (gradosEliminados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_EXTERNO, LOG_ERROR_GRADOS_ELIMINADOS);
                    return RedirectToAction("Index");
                }

                if (!validarEdicionAspirante(grados, modelo))
                    return await EditarPersonalExternoAsync(modelo);

                await procesarEdicionAspiranteAsync(modelo, gradosAgregados, gradosEditados, gradosEliminados);
                TempData["Success"] = string.Format(Constantes.TOAST_GUARDADO_EL, Constantes.PERSONAL_EXTERNO);

                return RedirectToAction("Index");
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, EDITAR_EXTERNO, Constantes.LOG_ERROR_JSON, vx.Message);
                return await EditarPersonalExternoAsync(modelo);
            }
            catch (JsonException jx)
            {
                this.LanzarError(_logger, jx, NOMBRE_LOGGER, EDITAR_EXTERNO, Constantes.LOG_ERROR_JSON);
                return RedirectToAction("Index");
            }
            catch (ArgumentNullException anx)
            {
                this.LanzarError(_logger, anx, NOMBRE_LOGGER, EDITAR_EXTERNO, string.Format(Constantes.LOG_GENERAL_NULO, anx.ParamName));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, EDITAR_EXTERNO, Constantes.LOG_ERROR_INESPERADO);
                return RedirectToAction("Index");
            }
        }

        private bool validarEdicionAspirante(List<DatosGradoDTO> grados, EditarDocenteViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                if (
                    string.IsNullOrEmpty(modelo.Nombre) ||
                    string.IsNullOrEmpty(modelo.DescripcionPerfil)
                   )
                    return false;
            }

            if (grados.Count < 1)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_EXTERNO, Constantes.LOG_ERROR_VALIDACION, "Agrega mínimo un Grado.");
                return false;
            }
            var hayUlimo = false;
            var cantidadUltimo = 0;
            foreach (var grado in grados)
            {
                if (grado.Ultimo)
                {
                    hayUlimo = true;
                    cantidadUltimo++;
                }
            }
            if (!hayUlimo)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_EXTERNO, Constantes.LOG_ERROR_VALIDACION, "Debe haber un Grado marcado como último.");
                return false;
            }
            if (cantidadUltimo > 1)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_EXTERNO, Constantes.LOG_ERROR_VALIDACION, "Sólo puede haber un Grado marcado como último.");
                return false;
            }

            return true;
        }

        private async Task procesarEdicionAspiranteAsync(EditarDocenteViewModel modelo, List<AgregarGradoDTO> gradosAgregados, List<DatosGradoDTO> gradosEditados, List<int> gradosEliminados)
        {
            var docenteDto = new EditarDocenteDTO
            {
                IdDocente = modelo.IdDocente,
                Nombre = modelo.Nombre,
                DescripcionPerfil = modelo.DescripcionPerfil,
                GradosAgregados = gradosAgregados,
                GradosEditados = gradosEditados,
                IdsGradosEliminados = gradosEliminados
            };

            if (modelo.Archivo is not null)
            {
                var (nombre, ruta) = await _archivoService.GuardarTemporalmenteAsync(modelo.Archivo);
                var archivoDTO = new CargarArchivoDTO
                {
                    NombreArchivo = nombre,
                    RutaArchivo = ruta
                };

                docenteDto.ArchivosGenerales = archivoDTO;
                docenteDto.NuevoArchivo = true;

                await _aspiranteService.EditarAspiranteAsync(docenteDto);
                System.IO.File.Delete(ruta);
            }
            else
                await _aspiranteService.EditarAspiranteAsync(docenteDto);
        }

        // ======================
        // VerDocumentosPersonal
        // ======================

        [HttpGet]
        public async Task<IActionResult> VerDocumentosPersonalAsync(int id, bool esDocente)
        {
            try
            {
                DatosDocenteDTO datos;
                if (esDocente)
                    datos = await _docenteService.ObtenerDocenteAsync(id);
                else
                    datos = await _aspiranteService.ObtenerAspiranteAsync(id);

                var modelo = new VerDocumentosPersonalViewModel
                {
                    IdArchivo = datos.IdArchivosGenerales,
                    NombrePersonal = datos.Nombre
                };

                return View("VerDocumentosPersonal", modelo);
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, VER_DOCUMENTOS, Constantes.LOG_ERROR_VALIDACION,
                    string.Format(Constantes.TOAST_ERROR_CARGAR_EL, esDocente ? Constantes.PERSONAL_ACADEMICO : Constantes.PERSONAL_EXTERNO));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, VER_DOCUMENTOS, Constantes.LOG_ERROR_INESPERADO);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPdfDocenteAsync(int idArchivo)
        {
            try
            {
                var archivo = await _archivoService.DescargarAsync(idArchivo);
                var bytes = await System.IO.File.ReadAllBytesAsync(archivo.Ruta);
                return File(bytes, archivo.Tipo, archivo.Nombre);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, "ObtenerPdfDocente:", Constantes.LOG_ERROR_INESPERADO);
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> DescargarArchivoAsync(int idArchivo)
        {
            try
            {
                var archivoDto = await _archivoService.DescargarAsync(idArchivo);
                if (archivoDto != null)
                {
                    var rutaCompleta = Path.Combine("Archivos/archivos-docente", archivoDto.Ruta);

                    if (System.IO.File.Exists(rutaCompleta))
                    {
                        var stream = new FileStream(rutaCompleta, FileMode.Open, FileAccess.Read);
                        return File(stream, archivoDto.Tipo, archivoDto.Nombre);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch
            {
                return BadRequest();
            }
        }

        // ==========
        // Utils
        // ==========
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
                            IsCheckBox = true,
                            Checked = grado.Ultimo
                        },
                        new()
                        {
                            Actions = new List<TableActionModel>
                            {
                                new()
                                {
                                    Accion = "editar",
                                    OnClick = $"abrirModalEditarGrado({grado.IdTemporal}, 0, '{grado.Grado}', '{grado.Titulo}', {grado.Ultimo.ToString().ToLower()})"
                                },
                                new()
                                {
                                    Accion = "eliminar",
                                    OnClick = $"abrirModalEliminarGrado({grado.IdTemporal})"
                                }
                            }
                        }
                    }
                }).ToList()
            };
        }

        private TableModel generarTablaGradosEdicion(List<DatosGradoDTO> lista)
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
                            IsCheckBox = true,
                            Checked = grado.Ultimo
                        },
                        new()
                        {
                            Actions = new List<TableActionModel>
                            {
                                new()
                                {
                                    Accion = "editar",
                                    OnClick = $"abrirModalEditarGrado({grado.IdTemporal}, {grado.IdGrado}, '{grado.Grado}', '{grado.Titulo}', {grado.Ultimo.ToString().ToLower()})"
                                },
                                new()
                                {
                                    Accion = "eliminar",
                                    OnClick = $"abrirModalEliminarGrado({(grado.IdTemporal > 0 ? grado.IdTemporal : grado.IdGrado)}, {(grado.IdTemporal > 0).ToString().ToLower()})"
                                }
                            }
                        }
                    }
                }).ToList()
            };
        }

        private List<OptionModel> generarListaPuestos(string puesto)
        {
            var listaPuestos = new List<OptionModel>();
            foreach (var p in Constantes.PUESTOS)
            {
                if (puesto is not null && p.Contains(puesto))
                    listaPuestos.Add(new OptionModel { Value = p, Text = p, Selected = true });
                else
                    listaPuestos.Add(new OptionModel { Value = p, Text = p });
            }
            return listaPuestos;
        }

        private List<OptionModel> generarListaGrados()
        {
            var listaGrados = new List<OptionModel>();
            foreach (var g in Constantes.GRADOS_DOCENTES)
                listaGrados.Add(new OptionModel { Value = g, Text = g });
            return listaGrados;
        }

        //Gestionar Grados Registro
        [HttpPost]
        public IActionResult AgregarGradoRegistro([FromBody] AgregarGradoDTO gradoDTO)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(gradoDTO, "Datos del Grado");

                var grados = obtenerDeSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS);
                if (grados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, "AgregarGradoRegistro:", LOG_ERROR_GRADOS_AGREGADOS);
                    return StatusCode(500);
                }

                grados = procesarAgregadoGradoRegistro(grados, gradoDTO);

                guardarEnSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS, grados);
                var tabla = generarTablaGradosRegistro(grados);
                return PartialView("_TablaGrados", tabla);
            }
            catch (JsonException jx)
            {
                this.LanzarError(_logger, jx, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_JSON);
                return StatusCode(500);
            }
            catch (ArgumentNullException anx)
            {
                this.LanzarError(_logger, anx, NOMBRE_LOGGER, EDITAR_ACADEMICO, string.Format(Constantes.LOG_GENERAL_NULO, anx.ParamName));

                if (anx.ParamName?.Contains("Datos del Grado") == true)
                    return BadRequest();
                else
                    return StatusCode(500);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_INESPERADO);
                return StatusCode(500);
            }
        }

        private List<AgregarGradoDTO> procesarAgregadoGradoRegistro(List<AgregarGradoDTO> grados, AgregarGradoDTO gradoDTO)
        {
            var idTemporal = 0;
            if (grados.Count > 0)
            {
                idTemporal = grados.Max(g => g.IdTemporal);
                idTemporal++;
            }

            gradoDTO.IdTemporal = idTemporal;
            grados.Add(gradoDTO);
            return grados;
        }

        [HttpPost]
        public IActionResult EditarGradoRegistro([FromBody] AgregarGradoDTO gradoDTO)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(gradoDTO, "Datos del Grado");

                var grados = obtenerDeSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS);
                if (grados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, "AgregarGradoRegistro:", LOG_ERROR_GRADOS_AGREGADOS);
                    return StatusCode(500);
                }

                grados = procesarEdicionGradoRegistro(grados, gradoDTO);

                guardarEnSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS, grados);
                var tabla = generarTablaGradosRegistro(grados);
                return PartialView("_TablaGrados", tabla);
            }
            catch (JsonException jx)
            {
                this.LanzarError(_logger, jx, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_JSON);
                return StatusCode(500);
            }
            catch (ArgumentNullException anx)
            {
                this.LanzarError(_logger, anx, NOMBRE_LOGGER, EDITAR_ACADEMICO, string.Format(Constantes.LOG_GENERAL_NULO, anx.ParamName));

                if (anx.ParamName?.Contains("Datos del Grado") == true)
                    return BadRequest();
                else
                    return StatusCode(500);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_INESPERADO);
                return StatusCode(500);
            }
        }

        private List<AgregarGradoDTO> procesarEdicionGradoRegistro(List<AgregarGradoDTO> grados, AgregarGradoDTO gradoDTO)
        {
            var gradoEditado = grados.FirstOrDefault(g => g.IdTemporal == gradoDTO.IdTemporal);
            ArgumentNullException.ThrowIfNull(gradoEditado, "Grado a editar");

            gradoEditado.Grado = gradoDTO.Grado;
            gradoEditado.Titulo = gradoDTO.Titulo;
            gradoEditado.Ultimo = gradoDTO.Ultimo;

            return grados;
        }

        [HttpGet]
        public IActionResult EliminarGradoRegistro(int idTemporal)
        {
            try
            {
                var grados = obtenerDeSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS);
                if (grados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, "AgregarGradoRegistro:", LOG_ERROR_GRADOS_AGREGADOS);
                    return StatusCode(500);
                }

                grados = procesarEliminadoGradoRegistro(grados, idTemporal);

                guardarEnSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS, grados);
                var tabla = generarTablaGradosRegistro(grados);
                return PartialView("_TablaGrados", tabla);
            }
            catch (JsonException jx)
            {
                this.LanzarError(_logger, jx, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_JSON);
                return StatusCode(500);
            }
            catch (ArgumentNullException anx)
            {
                this.LanzarError(_logger, anx, NOMBRE_LOGGER, EDITAR_ACADEMICO, string.Format(Constantes.LOG_GENERAL_NULO, anx.ParamName));
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_INESPERADO);
                return StatusCode(500);
            }
        }

        private List<AgregarGradoDTO> procesarEliminadoGradoRegistro(List<AgregarGradoDTO> grados, int idTemporal)
        {
            var gradoEliminado = grados.FirstOrDefault(g => g.IdTemporal == idTemporal);
            ArgumentNullException.ThrowIfNull(gradoEliminado);
            grados.Remove(gradoEliminado);
            return grados;
        }

        //Gestionar Grados Edicion
        [HttpPost]
        public IActionResult AgregarGradoEdicion([FromBody] AgregarGradoDTO agregarGradoDTO)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(agregarGradoDTO);

                var gradosAgregados = obtenerDeSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS);
                if (gradosAgregados == null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, LOG_ERROR_GRADOS_AGREGADOS);
                    return StatusCode(500);
                }

                var grados = obtenerDeSession<List<DatosGradoDTO>>(SESSION_GRADOS);
                if (grados == null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, LOG_ERROR_GRADOS);
                    return StatusCode(500);
                }

                (gradosAgregados, grados) = procesarAgregadoGradoEdicion(agregarGradoDTO, gradosAgregados, grados);

                guardarEnSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS, gradosAgregados);
                guardarEnSession<List<DatosGradoDTO>>(SESSION_GRADOS, grados);

                return PartialView("_TablaGrados", generarTablaGradosEdicion(grados));
            }
            catch (JsonException jx)
            {
                this.LanzarError(_logger, jx, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_JSON);
                return StatusCode(500);
            }
            catch (ArgumentNullException anx)
            {
                this.LanzarError(_logger, anx, NOMBRE_LOGGER, EDITAR_ACADEMICO, string.Format(Constantes.LOG_GENERAL_NULO, anx.ParamName));
                return BadRequest();
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_INESPERADO);
                return StatusCode(500);
            }
        }

        private (List<AgregarGradoDTO> gradosAgregados, List<DatosGradoDTO> grados) procesarAgregadoGradoEdicion(AgregarGradoDTO agregarGradoDTO, List<AgregarGradoDTO> gradosAgregados, List<DatosGradoDTO> grados)
        {
            var idTemporal = (gradosAgregados.Count > 0)
                ? gradosAgregados.Max(g => g.IdTemporal) + 1
                : 1;

            gradosAgregados.Add(new AgregarGradoDTO
            {
                IdTemporal = idTemporal,
                Grado = agregarGradoDTO.Grado,
                IdDocente = agregarGradoDTO.IdDocente,
                Titulo = agregarGradoDTO.Titulo,
                Ultimo = agregarGradoDTO.Ultimo
            });

            var gradoAgregado = new DatosGradoDTO
            {
                Grado = agregarGradoDTO.Grado,
                IdDocente = agregarGradoDTO.IdDocente,
                IdTemporal = idTemporal,
                Titulo = agregarGradoDTO.Titulo,
                Ultimo = agregarGradoDTO.Ultimo
            };
            grados.Add(gradoAgregado);

            return (gradosAgregados, grados);
        }

        [HttpPost]
        public IActionResult EditarGradoEdicion([FromBody] DatosGradoDTO datosGradoDTO)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(datosGradoDTO, "Datos del grado.");

                var gradosEditados = obtenerDeSession<List<DatosGradoDTO>>(SESSION_GRADOS_EDITADOS);
                if (gradosEditados == null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, LOG_ERROR_GRADOS_EDITADOS);
                    return StatusCode(500);
                }

                var gradosAgregados = obtenerDeSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS);
                if (gradosAgregados == null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, LOG_ERROR_GRADOS_AGREGADOS);
                    return StatusCode(500);
                }

                var grados = obtenerDeSession<List<DatosGradoDTO>>(SESSION_GRADOS);
                if (grados == null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, LOG_ERROR_GRADOS);
                    return StatusCode(500);
                }

                (gradosEditados, gradosAgregados, grados) = procesarEdicionGradoEdicion(datosGradoDTO, gradosEditados, gradosAgregados, grados);

                guardarEnSession<List<DatosGradoDTO>>(SESSION_GRADOS_EDITADOS, gradosEditados);
                guardarEnSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS, gradosAgregados);
                guardarEnSession<List<DatosGradoDTO>>(SESSION_GRADOS, grados);

                return PartialView("_TablaGrados", generarTablaGradosEdicion(grados));
            }
            catch (JsonException jx)
            {
                this.LanzarError(_logger, jx, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_JSON);
                return StatusCode(500);
            }
            catch (ArgumentNullException anx)
            {
                this.LanzarError(_logger, anx, NOMBRE_LOGGER, EDITAR_ACADEMICO, string.Format(Constantes.LOG_GENERAL_NULO, anx.ParamName));
                return BadRequest();
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_INESPERADO);
                return StatusCode(500);
            }
        }

        private (List<DatosGradoDTO> gradosEditados, List<AgregarGradoDTO> gradosAgregados, List<DatosGradoDTO> grados) procesarEdicionGradoEdicion(DatosGradoDTO datosGradoDTO, List<DatosGradoDTO> gradosEditados, List<AgregarGradoDTO> gradosAgregados, List<DatosGradoDTO> grados)
        {
            if (datosGradoDTO.IdTemporal > 0 && datosGradoDTO.IdGrado == 0)
            {
                var gradoNuevoEditado = gradosAgregados.FirstOrDefault(g => g.IdTemporal == datosGradoDTO.IdTemporal);
                ArgumentNullException.ThrowIfNull(gradoNuevoEditado, "Grado a editar en lista de Grados agregados.");

                var grado = grados.FirstOrDefault(g => g.IdTemporal == datosGradoDTO.IdTemporal);
                ArgumentNullException.ThrowIfNull(grado, "Grado a editar en lista de Grados.");

                gradoNuevoEditado.Grado = datosGradoDTO.Grado;
                gradoNuevoEditado.Titulo = datosGradoDTO.Titulo;
                gradoNuevoEditado.Ultimo = datosGradoDTO.Ultimo;

                grado.Grado = datosGradoDTO.Grado;
                grado.Titulo = datosGradoDTO.Titulo;
                grado.Ultimo = datosGradoDTO.Ultimo;
            }
            else if (datosGradoDTO.IdGrado > 0 && datosGradoDTO.IdTemporal == 0)
            {
                var grado = grados.FirstOrDefault(g => g.IdGrado == datosGradoDTO.IdGrado);
                ArgumentNullException.ThrowIfNull(grado, "Grado a editar en lista de Grados.");

                grado.Grado = datosGradoDTO.Grado;
                grado.Titulo = datosGradoDTO.Titulo;
                grado.Ultimo = datosGradoDTO.Ultimo;

                var gradoEditado = gradosEditados.FirstOrDefault(g => g.IdGrado == datosGradoDTO.IdGrado);
                if (gradoEditado is null)
                    gradosEditados.Add(datosGradoDTO);
                else
                {
                    gradoEditado.IdGrado = datosGradoDTO.IdGrado;
                    gradoEditado.IdDocente = datosGradoDTO.IdDocente;
                    gradoEditado.Grado = datosGradoDTO.Grado;
                    gradoEditado.Titulo = datosGradoDTO.Titulo;
                    gradoEditado.Ultimo = datosGradoDTO.Ultimo;
                }
            }
            else
            {
                throw new InvalidOperationException("Estado inválido del DTO.");
            }

            return (gradosEditados, gradosAgregados, grados);
        }

        [HttpGet]
        public IActionResult EliminarGradoEdicion(int idGrado, bool temporal)
        {
            try
            {
                if (idGrado < 1)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, "El IdGrado es inválido.");
                    return BadRequest();
                }
                var gradosEliminados = obtenerDeSession<List<int>>(SESSION_GRADOS_ELIMINADOS);
                if (gradosEliminados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, LOG_ERROR_GRADOS_ELIMINADOS);
                    return StatusCode(500);
                }
                var gradosEditados = obtenerDeSession<List<DatosGradoDTO>>(SESSION_GRADOS_EDITADOS);
                if (gradosEditados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, LOG_ERROR_GRADOS_EDITADOS);
                    return StatusCode(500);
                }
                var gradosAgregados = obtenerDeSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS);
                if (gradosAgregados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, LOG_ERROR_GRADOS_AGREGADOS);
                    return StatusCode(500);
                }
                var grados = obtenerDeSession<List<DatosGradoDTO>>(SESSION_GRADOS);
                if (grados is null)
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, LOG_ERROR_GRADOS);
                    return StatusCode(500);
                }

                (gradosEliminados, gradosEditados, gradosAgregados, grados) = procesarEliminadoGradoEdicion(idGrado, temporal, gradosEliminados, gradosEditados, gradosAgregados, grados);

                guardarEnSession<List<int>>(SESSION_GRADOS_ELIMINADOS, gradosEliminados);
                guardarEnSession<List<DatosGradoDTO>>(SESSION_GRADOS_EDITADOS, gradosEditados);
                guardarEnSession<List<AgregarGradoDTO>>(SESSION_GRADOS_AGREGADOS, gradosAgregados);
                guardarEnSession<List<DatosGradoDTO>>(SESSION_GRADOS, grados);

                return PartialView("_TablaGrados", generarTablaGradosEdicion(grados));
            }
            catch (JsonException jx)
            {
                this.LanzarError(_logger, jx, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_JSON);
                return StatusCode(500);
            }
            catch (ArgumentNullException anx)
            {
                this.LanzarError(_logger, anx, NOMBRE_LOGGER, EDITAR_ACADEMICO, string.Format(Constantes.LOG_GENERAL_NULO, anx.ParamName));
                return BadRequest();
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_INESPERADO);
                return StatusCode(500);
            }
        }

        private (List<int> gradosEliminados, List<DatosGradoDTO> gradosEditados, List<AgregarGradoDTO> gradosAgregados, List<DatosGradoDTO> grados) procesarEliminadoGradoEdicion (int idGrado, bool temporal, List<int> gradosEliminados, List<DatosGradoDTO> gradosEditados, List<AgregarGradoDTO> gradosAgregados, List<DatosGradoDTO> grados)
        {
            if (temporal)
            {
                var gradoNuevoEliminado = gradosAgregados.FirstOrDefault(g => g.IdTemporal == idGrado);
                ArgumentNullException.ThrowIfNull(gradoNuevoEliminado, "Grado a eliminar en lista de Grados nuevos.");
                var grado = grados.FirstOrDefault(g => g.IdTemporal == idGrado);
                ArgumentNullException.ThrowIfNull(grado, "Grado a eliminar en lista de Grados.");

                gradosAgregados.Remove(gradoNuevoEliminado);
                grados.Remove(grado);
            }
            else
            {
                var grado = grados.FirstOrDefault(g => g.IdGrado == idGrado);
                ArgumentNullException.ThrowIfNull(grado, "Grado a eliminar en lista de Grados.");
                grados.Remove(grado);

                var gradoEditadoEliminado = gradosEditados.FirstOrDefault(g => g.IdGrado == idGrado);
                if (gradoEditadoEliminado is not null)
                    gradosEditados.Remove(gradoEditadoEliminado);

                gradosEliminados.Add(idGrado);
            }

            return (gradosEliminados, gradosEditados, gradosAgregados, grados);
        }
    
        private T? obtenerDeSession<T>(string llave)
        {
            var json = HttpContext.Session.GetString(llave);

            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return default;
            }
        }

        private void guardarEnSession<T>(string llave, T objeto)
        {
            var json = JsonSerializer.Serialize(objeto);
            HttpContext.Session.SetString(llave, json);
        }
    }
}
