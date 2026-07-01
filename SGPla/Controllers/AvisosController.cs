using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Aviso;
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
                aviso = await _avisoService.ObtenerPorID((int)idAviso);
            }
            
            var articulos = await _articuloService.ObtenerTodosAsync();
            var articulosCombo = articulos
                .Select(a => new OptionModel
                {
                    Value = a.IdArticulo.ToString(),
                    Text = a.Numero,
                    Selected = aviso.idArticulo.HasValue && a.IdArticulo == idArticulo.Value
                })
                .ToList();

            var periodos = await _periodoService.ObtenerTodosAsync();
            var periodosCombo = periodos
                .Select(p => new OptionModel
                {
                    Value = p.IdPeriodoEscolar.ToString(),
                    Text = p.PeriodoMostrar,
                    Selected = idPeriodo.HasValue && p.IdPeriodoEscolar == idPeriodo.Value
                })
                .ToList();

            var modalidades = Constantes.MODALIDADES_AVISO;
            var modalidadesCombo = modalidades
                .Select(m => new OptionModel
                {
                    Value = m,
                    Text = m,
                    Selected = (m == modalidadSeleccionada)
                })
                .ToList();

            //var horarios = _avisoService.

            return new CrearAvisoViewModel
            {
                Periodos = periodosCombo,
                Articulos = articulosCombo,
                Modalidades = modalidadesCombo,
                TablaHorario = await LlenarTablaHorario()
            };
        }

        private async Task<TableModel> LlenarTablaHorario()
        {
            try
            {
                return new TableModel
                {
                    Headers = new List<string> {"Día", "Horario", "Acciones"},
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
                    }).ToList()
                };
            }
        }
    }
}
