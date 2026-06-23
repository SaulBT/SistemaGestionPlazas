using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Commons.Factories;
using SGPla.Models.Components;
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

        private const string GRADO_EDITADO = "Gado editado";
        private const string GRADO_ELIMINADO = "Grado eliminado";

        private static List<string> HEADERS_TABLA_DOCENTE = ["Nombre", "Número personal", "Puesto", "Acciones"];
        private static List<string> HEADERS_TABLA_ASPIRANTE = ["Nombre", "Grado", "Perfil", "Acciones"];
        private static List<string> HEADERS_TABLA_GRADOS = ["Grado", "Área", "Mayor grado de estudios", "Acciones"];

        private const string SESSION_GRADOS_AGREGADOS = "GradosAgregados";
        private const string SESSION_GRADOS_EDITADOS = "GradosEditados";
        private const string SESSION_GRADOS_ELIMINADOS = "GradosEliminados";

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
                Formluario = new FormularioGradoViewModel
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
            vista.Formluario = new FormularioGradoViewModel
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

        // =============
        // EditarDocente
        // =============

        //Vista
        //[HttpGet]
        //public IActionResult EditarPersonalInterno(EditarDocenteViewModel modelo)
        //{

        //}

        private async Task<EditarDocenteViewModel> inicializarEdicionDocenteAsync(int idDocente)
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
                modelo.Formluario = formularioGrados;
                modelo.Recarga = true;
                modelo.NuevoArchivo = false;

                HttpContext.Session.SetString(SESSION_GRADOS_AGREGADOS, JsonSerializer.Serialize(new List<AgregarGradoDTO>()));
                HttpContext.Session.SetString(SESSION_GRADOS_EDITADOS, JsonSerializer.Serialize(new List<DatosGradoDTO>()));
                HttpContext.Session.SetString(SESSION_GRADOS_ELIMINADOS, JsonSerializer.Serialize(new List<int>()));

                return modelo;
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, EDITAR_ACADEMICO, Constantes.LOG_ERROR_INESPERADO);
                return modelo;
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
    }
}
