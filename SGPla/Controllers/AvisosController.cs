using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Commons.Factories;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Aviso;
using SGPla.Models.DTOs.Horario;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Models.ViewModels.Avisos;
using SGPla.Services.Interfaces;

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
            var planes = await _avisoService.ObtenerPlanesConOfertasAviso
                (idEntidadAcademica, idPeriodo, idArticulo); //TODO

            if (planes.Count == 0)
            {

            }

            List<PlanEstudiosAvisoViewModel> planesViewModel = new List<PlanEstudiosAvisoViewModel>();
            foreach (var p in planes)
            {
                planesViewModel.Add(new PlanEstudiosAvisoViewModel
                {
                    Nombre = p.Nombre,
                    Tabla = await LlenarTablaOfertas(p.Ofertas)
                });
            }
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

        [HttpPost]
        public async Task<IActionResult> AgregarHorario([FromBody] List<CrearHorarioAvisoDTO> horarios)
        {
            var tabla = await ActualizarHorarios(horarios);

            return PartialView("_Horarios", tabla);
        }

        public async Task<TableModel> ActualizarHorarios(List<CrearHorarioAvisoDTO> horarios)
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
    }
}
