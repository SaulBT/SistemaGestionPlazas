using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Aviso;
using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Models.DTOs.Plantillas;
using Microsoft.IdentityModel.Tokens;
using SGPla.Commons;
using SGPla.Commons.Factories;
using SGPla.Models;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Aviso;
using SGPla.Models.DTOs.Horario;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Models.ViewModels.Avisos;
using SGPla.Services.Interfaces;
using SGPla.Views.Avisos;
using System.ComponentModel;
using System.Text.Json;

namespace SGPla.Controllers
{
    public class AvisosController : Controller
    {
        private readonly IAvisoService _avisoService;
        private readonly IPeriodoEscolarService _periodoService;
        private readonly IEntidadAcademicaService _entidadService;
        private readonly IArchivoService _archivoService;
        private readonly IArticuloService _articuloService;
        private readonly ILogger<AvisosController> _logger;
        private readonly IPlantillaService _plantillaService;
        private int _paginaActual = 1;
        private const int idEntidadAcademica = 1; //TODO: reemplazar al tener login

        private const string SESSION_ROL = "Rol";
        private const string SESSION_HORARIOS_AGREGADOS = "HorariosAgregados";
        private const string SESSION_OFERTAS = "Ofertas";

        private const string NOMBRE_LOGGER = "AVISOS-FRONT-";
        private const string INDEX = "index:";

        public AvisosController(
            IAvisoService avisoService,
            IPeriodoEscolarService periodoService,
            IArticuloService articuloService,
            IEntidadAcademicaService entidadService,
            IArchivoService archivoService,
            IPlantillaService plantillaService,
            ILogger<AvisosController> logger)
        {
            _avisoService = avisoService;
            _periodoService = periodoService;
            articuloService = articuloService;
            _entidadService = entidadService;
            _plantillaService = plantillaService;
            _archivoService = archivoService;
        }

        // ==========
        // INDEX
        // ==========

        //Vista
        public async Task<IActionResult> Index(string? busqueda, int? idPeriodo, int? idEntidadAcademica, DateOnly? fechaInicio, DateOnly? fechaFin, int cantidad = 10, int pagina = 1)
        {
            // CONFIGURAR EL ROL
            HttpContext.Session.SetString(SESSION_ROL, Constantes.COORDINADOR_EA);
            HttpContext.Session.SetInt32(Constantes.ID_ENTIDAD_ACADEMICA, 1);
            HttpContext.Session.SetInt32(Constantes.ID_AREA_ACADEMICA, 1);

            try
            {
                var periodos = await generarCatalogoPeriodosAsync(idPeriodo);
                var entidades = new List<OptionModel>();
                var avisos = new List<ListaAvisosDTO>();
                var total = 0;
                int? idAreaAcademica = 0;

                var rol = HttpContext.Session.GetString(SESSION_ROL);
                if (string.IsNullOrEmpty(rol))
                {
                    this.LanzarError(_logger, null, NOMBRE_LOGGER, INDEX, string.Format(Constantes.LOG_ERROR_NULO, SESSION_ROL));
                    return View(new IndexViewModel());
                }

                //Verificar rol
                if (rol.Contains(Constantes.COORDINADOR_EA))
                {
                    idEntidadAcademica = HttpContext.Session.GetInt32(Constantes.ID_ENTIDAD_ACADEMICA);
                    if (idEntidadAcademica is null)
                    {
                        this.LanzarError(_logger, null, NOMBRE_LOGGER, INDEX, string.Format(Constantes.LOG_ERROR_NULA, Constantes.ID_ENTIDAD_ACADEMICA));
                        return View(new IndexViewModel());
                    }
                }
                else if (rol.Contains(Constantes.COORDINADOR_DGAA))
                {
                    idAreaAcademica = HttpContext.Session.GetInt32(Constantes.ID_AREA_ACADEMICA);
                    if (idAreaAcademica is null)
                    {
                        this.LanzarError(_logger, null, NOMBRE_LOGGER, INDEX, string.Format(Constantes.LOG_ERROR_NULA, Constantes.ID_AREA_ACADEMICA));
                        return View(new IndexViewModel());
                    }
                    entidades = await generarCatalogoEntidadesAsync(idEntidadAcademica, idAreaAcademica.Value);
                }

                (avisos, total) = await _avisoService.ObtenerTodosAvisosAsync(new FiltroAvisosDTO
                {
                    Busqueda = busqueda,
                    IdEntidadAcademica = idEntidadAcademica,
                    IdPeriodo = idPeriodo,
                    FechaInicio = fechaInicio,
                    Cantidad = cantidad,
                    Pagina = pagina
                });

                TabAvisosViewModel todos = new() { Lista = avisos.Where(a => !a.Archivado).ToList() };
                TabAvisosViewModel creados = new() { Lista = avisos.Where(a => a.Estado.Contains(Constantes.CREADO) && !a.Archivado).ToList() };
                TabAvisosViewModel enRevision = new() { Lista = avisos.Where(a => a.Estado.Contains(Constantes.EN_REVISION_POR_DGAA) && !a.Archivado).ToList() };
                TabAvisosViewModel avalados = new() { Lista = avisos.Where(a => a.Estado.Contains(Constantes.AVALADO_POR_DGAA) && !a.Archivado).ToList() };
                TabAvisosViewModel devueltos = new() { Lista = avisos.Where(a => a.Estado.Contains(Constantes.DEVUELTO_POR_DGAA) && !a.Archivado).ToList() };
                TabAvisosViewModel firmados = new() { Lista = avisos.Where(a => a.Estado.Contains(Constantes.FIRMADO) && !a.Archivado).ToList() };
                TabAvisosViewModel publicados = new() { Lista = avisos.Where(a => a.Estado.Contains(Constantes.PUBLICADO) && !a.Archivado).ToList() };
                TabAvisosViewModel conActa = new() { Lista = avisos.Where(a => a.Estado.Contains(Constantes.ACTA_DE_CT_CREADA) && !a.Archivado).ToList() };
                TabAvisosViewModel archivados = new() { Lista = avisos.Where(a => a.Archivado).ToList() };

                return View(new IndexViewModel
                {
                    Periodos = periodos,
                    Entidades = entidades,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    IdEntidadSeleccionada = idEntidadAcademica,
                    IdPeriodoSeleccionado = idPeriodo,

                    Todos = todos,
                    Creados = creados,
                    EnRevision = enRevision,
                    Avalados = avalados,
                    Devueltos = devueltos,
                    Firmados = firmados,
                    Publicados = publicados,
                    ConActa = conActa,
                    Archivados = archivados,
                    Rol = rol,

                    PaginaActual = _paginaActual,
                    CantidadPorPagina = cantidad,
                });
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_VALIDACION, vx.Message);
                return View(new IndexViewModel());
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_INESPERADO);
                return View(new IndexViewModel());
            }
        }

        //Eliminar
        [HttpGet]
        public async Task EliminarAvisoAsync(int idAviso)
        {
            try
            {
                await _avisoService.EliminarAvisoPorId(idAviso);
                TempData["Success"] = string.Format(Constantes.TOAST_ELIMINACION_EL, Constantes.AVISO);
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_VALIDACION);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_INESPERADO);
            }
        }

        //Firmar
        [HttpPost]
        public async Task FirmarAvisoAsync([FromForm] int idAviso, [FromForm] IFormFile archivo)
        {
            try
            {
                (var nombre, var ruta) = await _archivoService.GuardarTemporalmenteAsync(archivo);
                var archivoDTO = new CargarArchivoDTO
                {
                    NombreArchivo = nombre,
                    RutaArchivo = ruta
                };
                await _avisoService.FirmarAvisoAsync(idAviso, archivoDTO);
                TempData["Success"] = "Aviso firmado con éxito.";
                System.IO.File.Delete(ruta);
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_VALIDACION);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_INESPERADO);
            }
        }

        //Publicar
        [HttpPost]
        public async Task PublicarAvisoAsync(int idAviso, string url)
        {
            try
            {
                await _avisoService.PublicarAvisoAsync(idAviso, url);
                TempData["Success"] = "Aviso publicado con éxito.";
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_VALIDACION);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_INESPERADO);
            }
        }

        //Archivar / Desarchivar
        [HttpPost]
        public async Task ArchivarAvisoAsync(int idAviso)
        {
            try
            {
                await _avisoService.ArchivarAvisoAsync(idAviso);
                TempData["Success"] = "Aviso archivado.";
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_VALIDACION);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_INESPERADO);
            }
        }

        [HttpPost]
        public async Task DesarchivarAvisoAsync(int idAviso)
        {
            try
            {
                await _avisoService.DesarchivarAvisoAsync(idAviso);
                TempData["Success"] = "Aviso desarchivado.";
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_VALIDACION);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_INESPERADO);
            }
        }

        // =================
        // Enviar a Revisión
        // =================

        //Vista
        [HttpGet]
        public async Task<IActionResult> EnviarARevisionAsync(int idAviso)
        {
            //TO DO
            return View();
        }

        [HttpPost]
        public async Task ConfirmarEnviarARevisionAsync(string comentarios, int idAviso)
        {
            try
            {
                var revisionDTO = new RevisionDTO
                {
                    Comentarios = comentarios,
                    IdAviso = idAviso
                };

                await _avisoService.EnviarARevisionAsync(revisionDTO);
                TempData["Success"] = "El Aviso se ha enviado a revisión por DGAA.";
            }
            catch (ValidacionExcepction vx)
            {
                this.LanzarError(_logger, vx, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_VALIDACION);
            }
            catch (Exception ex)
            {
                this.LanzarError(_logger, ex, NOMBRE_LOGGER, INDEX, Constantes.LOG_ERROR_INESPERADO);
            }
        }

        // ==========
        // UTILS
        // ==========

        private async Task<List<OptionModel>> generarCatalogoPeriodosAsync(int? idPeriodo)
        {
            var periodos = await _periodoService.ObtenerTodosAsync();
            return periodos.
                Select(p => new OptionModel
                {
                    Value = p.IdPeriodoEscolar.ToString(),
                    Text = p.PeriodoMostrar,
                    Selected = idPeriodo.HasValue && p.IdPeriodoEscolar == idPeriodo.Value
                })
                .ToList();
        }

        private async Task<List<OptionModel>> generarCatalogoEntidadesAsync(int? idEntidad, int idAreaAcademica)
        {
            var entidades = await _entidadService.ObtenerCatalogoAsync(new FiltroEntidadAcademicaDTO
            {
                IdAreaAcademica = idAreaAcademica
            });

            return entidades.
                Select(p => new OptionModel
                {
                    Value = p.IdEntidadAcademica.ToString(),
                    Text = p.Nombre,
                    Selected = idEntidad.HasValue && p.IdEntidadAcademica == idEntidad.Value
                })
                .ToList();
        }

        public async Task<IActionResult> CrearAviso()
        {
            return View(await ObtenerModelo());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearAviso(CrearAvisoViewModel model)
        {
            if (!validarFormulario(model))
                return View(await CargarCamposCrearAviso(model));

            
            try
            {
                var dto = new CrearAvisoDTO
                {
                    IdEntidadAcademica = idEntidadAcademica,
                    IdPeriodo = (int)model.IdPeriodo,
                    IdArticulo = (int)model.IdArticulo,
                    Folio = model.Folio,
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now),
                    FechaCT = DateOnly.Parse(model.FechaCT),
                    FechaVacantes = DateOnly.Parse(model.FechaVacantes),
                    Requisitos = model.Requisitos,
                    Lugar = model.Lugar,
                    Correo = model.Correo,
                    Modalidad = model.Modalidad,
                    OfertasId = obtenerDeSession<List<int>>(SESSION_OFERTAS),
                    Horarios = obtenerDeSession<List<CrearHorarioAvisoDTO>>(SESSION_HORARIOS_AGREGADOS)

                };


                await _avisoService.CrearAviso(dto);
                TempData["Success"] = "Aviso creado correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(await CargarCamposCrearAviso(model));
            }
        }

        private bool validarFormulario(CrearAvisoViewModel model)
        {
            if ((!model.Modalidad.IsNullOrEmpty())
                    && (model.Modalidad.Equals(Constantes.MODALIDAD_AVISO_PRESENCIAL))
                    && (model.Lugar.IsNullOrEmpty()))
            {
                ModelState.AddModelError("Lugar", Constantes.CAMPO_OBLIGATORIO);
            }
            if (!ModelState.IsValid)
            {
                TempData["Warning"] = "Formulario incompleto";
                return false;
            }

            var horarios = obtenerDeSession<List<CrearHorarioAvisoDTO>>(SESSION_HORARIOS_AGREGADOS);
            if ((horarios.IsNullOrEmpty()) || (horarios.Count == 0))
            {
                TempData["Warning"] = "Seleccione por lo menos un Horario";
                return false;
            }
            
            var ofertas = obtenerDeSession<List<int>>(SESSION_OFERTAS);
            if ((ofertas.IsNullOrEmpty()) || (ofertas.Count == 0))
            {
                TempData["Warning"] = "No hay ofertas seleccionadas";
                return false;
            }
            return true;
        }

        private async Task<CrearAvisoViewModel> CargarCamposCrearAviso (CrearAvisoViewModel model)
        {
            CrearAvisoViewModel nuevoModelo = await ObtenerModelo();

            nuevoModelo.Lugar = model.Lugar ?? nuevoModelo.Lugar;
            nuevoModelo.Correo = model.Correo ?? nuevoModelo.Correo;
            nuevoModelo.FechaCT = model.FechaCT ?? nuevoModelo.FechaCT;
            
            nuevoModelo.FechaVacantes = model.FechaVacantes ?? nuevoModelo.FechaVacantes;
            nuevoModelo.Horarios = model.Horarios ?? nuevoModelo.Horarios;
            nuevoModelo.IdPeriodo = model.IdPeriodo ?? nuevoModelo.IdPeriodo;
            nuevoModelo.Modalidad = model.Modalidad ?? nuevoModelo.Modalidad;
            nuevoModelo.IdArticulo = model.IdArticulo ?? nuevoModelo.IdArticulo;
            nuevoModelo.Folio = model.Folio ?? nuevoModelo.Folio;

            return nuevoModelo;

        }

        private async Task<CrearAvisoViewModel> ObtenerModelo(int? idAviso = null)
        {
            DatosAvisoDTO aviso = new DatosAvisoDTO();
            if (idAviso != null)
            {
                aviso = await _avisoService.ObtenerAvisoPorIDAsync((int)idAviso);
            }
            
            var articulos = await _articuloService.ObtenerTodosAsync();
            var articulosCombo = articulos
                .Select(a => new OptionModel
                {
                    Value = a.IdArticulo.ToString(),
                    Text = a.Numero,
                    Selected = aviso.IdArticulo > 0 && a.IdArticulo == aviso.IdArticulo
                })
                .ToList();

             var periodos = await _periodoService.ObtenerTodosAsync();
            var periodosCombo = periodos
                .Select(p => new OptionModel
                {
                    Value = p.IdPeriodoEscolar.ToString(),
                    Text = p.PeriodoMostrar,
                    Selected = aviso.IdPeriodo > 0 && p.IdPeriodoEscolar == aviso.IdPeriodo
                })
                .ToList();

            var modalidades = Constantes.MODALIDADES_AVISO;
            var modalidadesCombo = modalidades
                .Select(m => new OptionModel
                {
                    Value = m,
                    Text = m,
                    Selected = (m == aviso.Modalidad)
                })
                .ToList();


            return new CrearAvisoViewModel
            {
                Periodos = periodosCombo,
                Articulos = articulosCombo,
                Modalidades = modalidadesCombo,
                TablaHorario = await LlenarTablaHorario(aviso.Horarios),
                PlanesEstudios = await CargarPlanesEstudios(-1, -1)
            };
        }

        private async Task<List<PlanEstudiosAvisoViewModel>> CargarPlanesEstudios(int idPeriodo, int idArticulo)
        {
            var planes = await _avisoService.ObtenerPlanesConOfertasAviso //Actualmente retorna las ofertas por Programa educativo
                (idEntidadAcademica, idPeriodo, idArticulo); //TODO

            List<int> idOfertas = new List<int>();
            List<PlanEstudiosAvisoViewModel> planesViewModel = new List<PlanEstudiosAvisoViewModel>();
            foreach (var p in planes)
            {
                planesViewModel.Add(new PlanEstudiosAvisoViewModel
                {
                    Nombre = p.Nombre,
                    Tabla = await LlenarTablaOfertas(p.Ofertas)
                });
                foreach (var o in p.Ofertas)
                {
                    idOfertas.Add(o.IdOferta);
                }
                
            }
            guardarEnSession<List<int>>(SESSION_OFERTAS, idOfertas);

            return planesViewModel;
        }

        private async Task<TableModel> LlenarTablaOfertas(List<DatosOfertaAvisoDTO> ofertas)
        {
            try
            {
                List<string> headers = new List<string> 
                { "Horas", "EE", "NRC", "Plaza", "Horario", "Tipo de Contratacion", "Perfil Docente" };

                if (ofertas.Count == 0)
                    return TablaFactory.GenerarTablaConMensajeSinPaginacion(headers, "Error, esto no debería esta vacío.");

                return new TableModel
                {
                    Headers = headers,
                    Rows = ofertas.Select(o => new TableRowModel
                    {
                        Cells = new List<TableCellModel>
                        {
                            new() { Value = o.Horas.ToString() },
                            new() { Value = o.ExperienciaEducativa },
                            new() { Value = o.NRC.ToString()},
                            new() { Value = o.Plaza},
                            new()
                            {
                                Actions = new List<TableActionModel>
                                {
                                    new TableActionModel()
                                    {
                                        Accion = "Info",
                                        OnClick = $"verHorarioOferta()"
                                    }
                                }
                            },
                            new() { Value = o.TipoContratacion},
                            new() 
                            {
                                Actions = new List<TableActionModel>
                                {
                                    new TableActionModel()
                                    {
                                        Accion = "Info",
                                        OnClick = $"verPerfilDocenteOferta()"
                                    }
                                }
                            },
                        }
                    }).ToList(),
                    Pagination = new PaginationInfo
                    {
                        PaginationMode = "NA"
                    }
                };
            }catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llenar la tabla de ofertas");
                TempData["Error"] = ex.Message;

                return new TableModel();
            }
        }

        private async Task<TableModel> LlenarTablaHorarioCrearAviso(List<CrearHorarioAvisoDTO> horarios)
        {
            try
            {
                List<string> headers = new List<string> { "Día", "Horario", "Acciones" };

                if (horarios.Count() == 0)
                    return TablaFactory.GenerarTablaConMensajeSinPaginacion(headers, "No ha ingresado ningún horario");
                return new TableModel
                {
                    Headers = headers,
                    Rows = horarios.Select(h => new TableRowModel
                    {
                        Cells = new List<TableCellModel>
                        {
                            new() { Value = "Lunes"},
                            new() { Value = "Prueba"},
                            new()
                            {
                                Actions = new List<TableActionModel>
                                {
                                    new TableActionModel()
                                    {
                                        Accion = "Editar",
                                        OnClick = $"abrirModalEditarHorario()"
                                    },
                                    new TableActionModel()
                                    {
                                        Accion = "eliminar",
                                        OnClick = $"abrirModalConfirmacion('¿Desea eliminar este horario?', function() {{ eliminarHorario(); }} )"
                                    }
                                }
                            }
                        },
                    }).ToList(),
                    Pagination = new PaginationInfo
                    {
                        PaginationMode = "NA"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llenar la tabla de horarios");
                TempData["Error"] = ex.Message;

                return new TableModel();
            }
        }

        private async Task<TableModel> LlenarTablaHorario(List<DatosHorarioDTO> horarios)
        {
            try
            {
                List<string> headers = new List<string> { "Día", "Horario", "Acciones" };

                if (horarios.Count() == 0)
                    return TablaFactory.GenerarTablaConMensajeSinPaginacion(headers, "No ha ingresado ningún horario");
                return new TableModel
                {
                    Headers = headers,
                    Rows = horarios.Select( h => new TableRowModel
                    {
                        Cells = new List<TableCellModel>
                        {
                            new() { Value = "Lunes"},
                            new() { Value = "Prueba"},
                            new()
                            {
                                Actions = new List<TableActionModel>
                                {
                                    new TableActionModel()
                                    {
                                        Accion = "Editar",
                                        OnClick = $"abrirModalEditarHorario()"
                                    },
                                    new TableActionModel()
                                    {
                                        Accion = "eliminar",
                                        OnClick = $"abrirModalConfirmacion('¿Desea eliminar este horario?', function() {{ eliminarHorario(); }} )"
                                    }
                                }
                            }
                        },
                    }).ToList(),
                    Pagination = new PaginationInfo
                    {
                        PaginationMode = "NA"
                    }
                };
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llenar la tabla de horarios");
                TempData["Error"] = ex.Message;

                return new TableModel();
            }
        }

        [HttpGet]
        public async Task<IActionResult> ActualizarOfertasAsync(int idPeriodo, int idArticulo)
        {
            var ofertas = await CargarPlanesEstudios(idPeriodo, idArticulo);
            return PartialView("_TablasOfertas", ofertas);
        }

        [HttpGet]
        public IActionResult ObtenerHorarios()
        {
            var horarios = obtenerDeSession<List<CrearHorarioAvisoDTO>>(SESSION_HORARIOS_AGREGADOS);
            horarios ??= new List<CrearHorarioAvisoDTO>();
            return Json(horarios);
        }

        [HttpPost]
        public async Task<IActionResult> AgregarHorario([FromBody] List<CrearHorarioAvisoDTO> horarios)
        {
            guardarEnSession<List<CrearHorarioAvisoDTO>>(SESSION_HORARIOS_AGREGADOS, horarios);
            var tabla = await ActualizarTablaHorarios(horarios);
            return PartialView("_Horarios", tabla);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTablaHorarios()
        {
            var horarios = obtenerDeSession<List<CrearHorarioAvisoDTO>>(SESSION_HORARIOS_AGREGADOS);

            horarios ??= new List<CrearHorarioAvisoDTO>();

            var tabla = await ActualizarTablaHorarios(horarios);

            return PartialView("_Horarios", tabla);
        }

        public async Task<TableModel> ActualizarTablaHorarios(List<CrearHorarioAvisoDTO> horarios)
        {
            try
            {
                List<string> headers = new List<string> { "Día", "Horario", "Acciones" };
                if (horarios.Count == 0)
                    return TablaFactory.GenerarTablaConMensajeSinPaginacion(headers, "Ingrese como mínimo 1 horario.");
                return new TableModel
                {
                    Headers = headers,
                    Rows = horarios.Select(h => new TableRowModel
                    {
                        Cells = new List<TableCellModel>
                        {
                            new() { Value = h.Fecha},
                            new() { Value = h.HoraInicio +" - "+h.HoraTermino },
                            new()
                            {
                                Actions = new List<TableActionModel>
                                {
                                    new TableActionModel
                                    {
                                        Accion = "Editar",
                                        OnClick = $"editarHorario()"
                                    },
                                    new TableActionModel
                                    {
                                        Accion = "Eliminar",
                                        OnClick = $"eliminarHorario()"
                                    }
                                }
                            }
                        }
                    }).ToList(),
                    Pagination = new PaginationInfo
                    {
                        PaginationMode = "NA"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llenar la tabla de horarios");
                TempData["Error"] = ex.Message;

                return new TableModel();
            }
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
