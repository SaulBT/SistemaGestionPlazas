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

        private const string LOG_ERROR_GRADOS_AGREGADOS = "No se pudo cargar la lista de Grados agregados.";
        private const string LOG_ERROR_GRADOS_EDITADOS = "No se pudo cargar la lista de Grados editados.";
        private const string LOG_ERROR_GRADOS_ELIMINADOS = "No se pudo cargar la lista de Grados eliminados";
        private const string LOG_ERROR_GRADOS = "No se pudo cargar la lista de Grados";

        private const string GRADO_EDITADO = "Gado editado";
        private const string GRADO_ELIMINADO = "Grado eliminado";

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
                                        Url = Url.Action("Docentes", "VerDocumentos")
                                    },
                                    new()
                                    {
                                        Accion = "editar",
                                        Url = Url.Action("EditarPersonalAcademico", "Docentes", new {modelo = new EditarDocenteViewModel{ IdDocente = docente.IdDocente } })
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
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, INDEX, "No se pudo cargar la lista de Aspirante");
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

                return View("RegistrarPersonalAcademico", vista);
            }  

            if (error)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, REGISTRAR_ACADEMICO, LOG_ERROR_GRADOS_AGREGADOS);
                return View("Index");
            }

            return View("RegistrarPersonalAcademico", inicializarRegistroDocente());
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

            HttpContext.Session.SetString(SESSION_GRADOS_AGREGADOS, JsonSerializer.Serialize(new List<AgregarGradoDTO>()));

            return modelo;
        }

        private (RegistrarDocenteViewModel vista, bool error) recargargarRegistroDocente(RegistrarDocenteViewModel modelo)
        {
            var gradosAgregados = HttpContext.Session.GetString(SESSION_GRADOS_AGREGADOS);
            if (!string.IsNullOrEmpty(gradosAgregados))
            {
                var listaGrados = JsonSerializer.Deserialize<List<AgregarGradoDTO>>(gradosAgregados);
                modelo.OpcionesPuesto = generarListaPuestos(modelo.Puesto);
                modelo.Tabla = generarTablaGradosRegistro(listaGrados);

                return (modelo, false);
            }
            else
                return (modelo, true);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarDocenteAsync(RegistrarDocenteViewModel modelo)
        {
            try
            {
                var gradosJson = HttpContext.Session.GetString(SESSION_GRADOS_AGREGADOS);
                if (!string.IsNullOrEmpty(gradosJson))
                {
                    var grados = JsonSerializer.Deserialize<List<AgregarGradoDTO>>(gradosJson);
                    if (!validarRegistro(grados, modelo.Archivo, REGISTRAR_ACADEMICO))
                        return await RegistrarPersonalAcademicoAsync(modelo);

                    var (nombre, ruta) = await _archivoService.GuardarTemporalmenteAsync(modelo.Archivo);

                    var dto = new RegistrarDocenteDTO
                    {
                        Nombre = modelo.Nombre,
                        DescripcionPerfil = modelo.DescripcionPerfil,
                        ArchivosGenerales = new Models.DTOs.Archivo.CargarArchivoDTO
                        {
                            NombreArchivo = nombre,
                            RutaArchivo = ruta
                        },
                        Grados = grados,
                        NumeroPersonal = modelo.NumeroPersonal,
                        Puesto = modelo.Puesto
                    };
                    await _docenteService.RegistrarDocenteAsync(dto);
                    TempData["Success"] = string.Format(Constantes.TOAST_GUARDADO_EL, Constantes.PERSONAL_ACADEMICO);
                    System.IO.File.Delete(ruta);
                    return RedirectToAction("Index");
                }
                else
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, REGISTRAR_ACADEMICO, LOG_ERROR_GRADOS_AGREGADOS);
                    return RedirectToAction(nameof(IndexAsync));
                }
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, REGISTRAR_ACADEMICO, Constantes.LOG_ERROR_VALIDACION);
                return await RegistrarPersonalAcademicoAsync(modelo);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, REGISTRAR_ACADEMICO, Constantes.LOG_ERROR_INESPERADO);
                return await RegistrarPersonalAcademicoAsync(modelo);
            }
        }

        // ====================
        // RegistrarAspirante
        // ====================

        //Vista
        [HttpGet]
        public IActionResult RegistrarPersonalExterno(RegistrarDocenteViewModel modelo)
        {
            var vista = new RegistrarDocenteViewModel();
            vista.Formulario = new FormularioGradoViewModel
            {
                ListaGrados = generarListaGrados()
            };

            if (modelo.Recarga)
                return View(modelo);
            else
            {
                vista.Tabla = TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_GRADOS, "No se han agregado grados.");
                vista.Recarga = true;
                HttpContext.Session.SetString(SESSION_GRADOS_AGREGADOS, string.Empty);

                return View(vista);
            }
        }

        public async Task<IActionResult> GuardarAspiranteAsync(RegistrarDocenteViewModel modelo)
        {
            try
            {
                var gradosJson = HttpContext.Session.GetString(SESSION_GRADOS_AGREGADOS);
                if (!string.IsNullOrEmpty(gradosJson))
                {
                    var grados = JsonSerializer.Deserialize<List<AgregarGradoDTO>>(gradosJson);
                    if (!validarRegistro(grados, modelo.Archivo, REGISTRAR_EXTERNO))
                        return RegistrarPersonalExterno(modelo);

                    var (nombre, ruta) = await _archivoService.GuardarTemporalmenteAsync(modelo.Archivo);

                    var dto = new RegistrarDocenteDTO
                    {
                        Nombre = modelo.Nombre,
                        DescripcionPerfil = modelo.DescripcionPerfil,
                        ArchivosGenerales = new Models.DTOs.Archivo.CargarArchivoDTO
                        {
                            NombreArchivo = nombre,
                            RutaArchivo = ruta
                        },
                        Grados = grados
                    };
                    await _aspiranteService.RegistrarAspiranteAsync(dto);
                    TempData["Success"] = string.Format(Constantes.TOAST_GUARDADO_EL, Constantes.PERSONAL_EXTERNO);
                    return RedirectToAction(nameof(IndexAsync));
                }
                else
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, REGISTRAR_EXTERNO, LOG_ERROR_GRADOS_AGREGADOS);
                    return RedirectToAction(nameof(IndexAsync));
                }
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, REGISTRAR_EXTERNO, Constantes.LOG_ERROR_VALIDACION);
                return await RegistrarPersonalAcademicoAsync(modelo);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, REGISTRAR_ACADEMICO, Constantes.LOG_ERROR_INESPERADO);
                return await RegistrarPersonalAcademicoAsync(modelo);
            }
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

                var formularioGrados = new FormularioGradoViewModel
                {
                    ListaGrados = generarListaGrados()
                };
                modelo.Formulario = formularioGrados;
                modelo.Recarga = true;
                modelo.NuevoArchivo = false;

                HttpContext.Session.SetString(SESSION_GRADOS_AGREGADOS, JsonSerializer.Serialize(new List<AgregarGradoDTO>()));
                HttpContext.Session.SetString(SESSION_GRADOS_EDITADOS, JsonSerializer.Serialize(new List<DatosGradoDTO>()));
                HttpContext.Session.SetString(SESSION_GRADOS_ELIMINADOS, JsonSerializer.Serialize(new List<int>()));
                HttpContext.Session.SetString(SESSION_GRADOS, JsonSerializer.Serialize(new List<DatosGradoDTO>()));

                return (modelo, false);
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_VALIDACION, string.Format(Constantes.TOAST_ERROR_CARGAR_EL, Constantes.PERSONAL_ACADEMICO));
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
            if (modelo != null)
            {
                var formularioGrados = new FormularioGradoViewModel
                {
                    ListaGrados = generarListaGrados()
                };
                modelo.Tabla = TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_GRADOS, string.Format(Constantes.ERROR_TABLA, Constantes.GRADOS));
                modelo.OpcionesPuesto = generarListaPuestos(modelo.Puesto);
                modelo.Formulario = formularioGrados;

                var gradosJson = HttpContext.Session.GetString(SESSION_GRADOS);
                if (!string.IsNullOrEmpty(gradosJson))
                {
                    var grados = JsonSerializer.Deserialize<List<DatosGradoDTO>>(gradosJson);
                    modelo.Tabla = generarTablaGradosEdicion(grados);

                    return (modelo, false);
                }
                else
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, LOG_ERROR_GRADOS, string.Format(Constantes.TOAST_ERROR_GUARDAR_EL, Constantes.PERSONAL_ACADEMICO));
            }
            else
                this.LanzarError(_logger, null, NOMBRE_LOGGER, EDITAR_ACADEMICO, "No se enviaron datos al recargar la vista.", string.Format(Constantes.TOAST_ERROR_GUARDAR_EL, Constantes.PERSONAL_ACADEMICO));

            return (new EditarDocenteViewModel(), true);
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

                if (!validarEdicion(grados, modelo.Archivo, EDITAR_ACADEMICO))
                    return await EditarPersonalAcademicoAsync(modelo);

                await procesarEdicionDocenteAsync(modelo, gradosAgregados, gradosEditados, gradosEliminados);

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

            if (modelo.NuevoArchivo)
            {
                var (nombre, ruta) = await _archivoService.GuardarTemporalmenteAsync(modelo.Archivo);
                var archivoDTO = new CargarArchivoDTO
                {
                    NombreArchivo = nombre,
                    RutaArchivo = ruta
                };

                docenteDto.ArchivosGenerales = archivoDTO;
            }

            await _docenteService.EditarDocenteAsync(docenteDto);
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
                                    OnClick = $"abrirModalEditarGrado('{grado.IdTemporal}', '{grado.Grado}', '{grado.Titulo}', {grado.Ultimo.ToString().ToLower()})"
                                },
                                new()
                                {
                                    Accion = "eliminar",
                                    OnClick = $"abrirModalEliminarGrado('{grado.IdTemporal}')"
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
                                    OnClick = $"abrirModalEditarGrado('{grado.IdGrado}', '{grado.Grado}', '{grado.Titulo}', {grado.Ultimo.ToString().ToLower()})"
                                },
                                new()
                                {
                                    Accion = "eliminar",
                                    OnClick = $"abrirModalEliminarGrado('{grado.IdGrado}')"
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
                if (p.Contains(puesto))
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

        private bool validarRegistro(List<AgregarGradoDTO> grados, IFormFile archivo, string metodo)
        {
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
                this.LanzarError(_logger, null, NOMBRE_LOGGER, metodo, Constantes.LOG_ERROR_VALIDACION, "Debe haber un Grado marcado como último.");
                return false;
            }
            if (cantidadUltimo > 1)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, metodo, Constantes.LOG_ERROR_VALIDACION, "Sólo puede haber un Grado marcado como último.");
                return false;
            }

            if (archivo == null)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, metodo, Constantes.LOG_ERROR_VALIDACION, "Debes cargar un archivo.");
                return false;
            }

            return true;
        }

        private bool validarEdicion(List<DatosGradoDTO> grados, IFormFile archivo, string metodo)
        {
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
                this.LanzarError(_logger, null, NOMBRE_LOGGER, metodo, Constantes.LOG_ERROR_VALIDACION, "Debe haber un Grado marcado como último.");
                return false;
            }
            if (cantidadUltimo > 1)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, metodo, Constantes.LOG_ERROR_VALIDACION, "Sólo puede haber un Grado marcado como último.");
                return false;
            }

            if (archivo == null)
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, metodo, Constantes.LOG_ERROR_VALIDACION, "Debes cargar un archivo.");
                return false;
            }

            return true;
        }

        //Gestionar Grados Registro
        [HttpGet]
        public IActionResult AgregarGradoRegistro(string grado, string titulo, bool ultimo)
        {
            bool error = false;
            AgregarGradoDTO gradoAgregado = new();
            var tabla = new TableModel();

            var gradosJson = HttpContext.Session.GetString(SESSION_GRADOS_AGREGADOS);
            if (!string.IsNullOrEmpty(gradosJson))
            {
                var grados = JsonSerializer.Deserialize<List<AgregarGradoDTO>>(gradosJson);
                var idTemporal = 0;
                if (grados.Count > 0)
                {
                    idTemporal = grados.Max(g => g.IdTemporal);
                    idTemporal++;
                }

                gradoAgregado = new AgregarGradoDTO
                {
                    IdTemporal = idTemporal,
                    Grado = grado,
                    Titulo = titulo,
                    Ultimo = ultimo
                };

                grados.Add(gradoAgregado);
                gradosJson = JsonSerializer.Serialize(grados);
                HttpContext.Session.SetString(SESSION_GRADOS_AGREGADOS, gradosJson);
                tabla = generarTablaGradosRegistro(grados);
                return PartialView("_TablaGrados", tabla);
            }
            else
            {
                this.LanzarError(_logger, null, NOMBRE_LOGGER, "AgregarGradoRegistro:", LOG_ERROR_GRADOS_AGREGADOS);
                return RedirectToAction(nameof(IndexAsync));
            }
        }

        [HttpGet]
        public IActionResult EditarGradoRegistro(int idTemporal, string grado, string titulo, bool ultimo)
        {
            bool error = false;
            var tabla = new TableModel();

            var gradosJson = HttpContext.Session.GetString(SESSION_GRADOS_AGREGADOS);
            if (!string.IsNullOrEmpty(gradosJson))
            {
                var grados = JsonSerializer.Deserialize<List<AgregarGradoDTO>>(gradosJson);
                var gradoEditado = grados.FirstOrDefault(g => g.IdTemporal == idTemporal);
                if (gradoEditado != null)
                {
                    gradoEditado.Grado = grado;
                    gradoEditado.Titulo = titulo;
                    gradoEditado.Ultimo = ultimo;

                    gradosJson = JsonSerializer.Serialize(grados);
                    HttpContext.Session.SetString(SESSION_GRADOS_AGREGADOS, gradosJson);
                    tabla = generarTablaGradosRegistro(grados);
                }
                else
                {
                    error = true;
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, "EditarGradoRegistro:", string.Format(Constantes.LOG_ERROR_NULO, GRADO_EDITADO));
                }
            }
            else
            {
                error = true;
                this.LanzarError(_logger, null, NOMBRE_LOGGER, "EditarGradoRegistro:", LOG_ERROR_GRADOS_AGREGADOS);
            }

            if (error)
                return RedirectToAction(nameof(IndexAsync));

            return PartialView("_TablaGrados", tabla);
        }

        [HttpGet]
        public IActionResult EliminarGradoRegistro(int idTemporal)
        {
            bool error = false;
            var tabla = new TableModel();

            var gradosJson = HttpContext.Session.GetString(SESSION_GRADOS_AGREGADOS);
            if (!string.IsNullOrEmpty(gradosJson))
            {
                var grados = JsonSerializer.Deserialize<List<AgregarGradoDTO>>(gradosJson);
                var gradoEliminado = grados.FirstOrDefault(g => g.IdTemporal == idTemporal);
                if (gradoEliminado != null)
                {
                    grados.Remove(gradoEliminado);
                    gradosJson = JsonSerializer.Serialize(grados);
                    HttpContext.Session.SetString(SESSION_GRADOS_AGREGADOS, gradosJson);
                    tabla = generarTablaGradosRegistro(grados);
                }
                else
                {
                    error = true;
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, "EliminarGradoRegistro:", string.Format(Constantes.LOG_ERROR_NULO, GRADO_ELIMINADO));
                }
            }
            else
            {
                error = true;
                this.LanzarError(_logger, null, NOMBRE_LOGGER, "EliminarGradoRegistro:", LOG_ERROR_GRADOS_AGREGADOS);
            }

            if (error)
                return RedirectToAction(nameof(IndexAsync));

            return PartialView("_TablaGrados", tabla);
        }

        //Gestionar Grados Edicion
        [HttpGet]
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
                Titulo = agregarGradoDTO.Titulo,
                Ultimo = agregarGradoDTO.Ultimo
            };
            grados.Add(gradoAgregado);

            return (gradosAgregados, grados);
        }

        [HttpGet]
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
                var gradoEditado = gradosEditados.FirstOrDefault(g => g.IdGrado == datosGradoDTO.IdGrado);
                ArgumentNullException.ThrowIfNull(gradoEditado, "Grado a editar en lista de Grados editados.");

                var grado = grados.FirstOrDefault(g => g.IdGrado == datosGradoDTO.IdGrado);
                ArgumentNullException.ThrowIfNull(grado, "Grado a editar en lista de Grados.");

                gradoEditado.Grado = datosGradoDTO.Grado;
                gradoEditado.Titulo = datosGradoDTO.Titulo;
                gradoEditado.Ultimo = datosGradoDTO.Ultimo;

                grado.Grado = datosGradoDTO.Grado;
                grado.Titulo = datosGradoDTO.Titulo;
                grado.Ultimo = datosGradoDTO.Ultimo;
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
                var gradoEditadoEliminado = gradosEditados.FirstOrDefault(g => g.IdGrado == idGrado);
                ArgumentNullException.ThrowIfNull(gradoEditadoEliminado, "Grado a eliminar en lista de Grados editados.");
                var grado = grados.FirstOrDefault(g => g.IdGrado == idGrado);
                ArgumentNullException.ThrowIfNull(grado, "Grado a eliminar en lista de Grados.");

                gradosEditados.Remove(gradoEditadoEliminado);
                grados.Remove(grado);
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
