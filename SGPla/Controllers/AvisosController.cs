using Microsoft.AspNetCore.Mvc;
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
        private readonly IArticuloService _articuloService;
        private readonly ILogger<AvisosController> _logger;
        private int _paginaActual = 1;
        private const int idEntidadAcademica = 1; //TODO: reemplazar al tener login

        private const string SESSION_HORARIOS_AGREGADOS = "HorariosAgregados";
        private const string SESSION_OFERTAS = "Ofertas";



        public AvisosController(IAvisoService avisoService, IPeriodoEscolarService periodoService, IArticuloService articuloService, ILogger<AvisosController> logger)
        {
            _avisoService = avisoService;
            _periodoService = periodoService;
            _articuloService = articuloService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? busqueda, int? idPeriodo, DateOnly? fechaInicio, DateOnly? fechaFin, int cantidad = 10)
        {
            var periodos = await _periodoService.ObtenerTodosAsync();
            var periodosCombo = periodos.
                Select(p => new OptionModel
                {
                    Value = p.IdPeriodoEscolar.ToString(),
                    Text = p.PeriodoMostrar,
                    Selected = idPeriodo.HasValue && p.IdPeriodoEscolar == idPeriodo.Value
                })
                .ToList();

            return View(new IndexViewModel
            {
                Periodos = periodosCombo,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                IdPeriodoSeleccionado = idPeriodo,
                PaginaActual = _paginaActual,
                CantidadPorPagina = cantidad,
            });
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
