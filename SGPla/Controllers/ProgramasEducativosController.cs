using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models;
using SGPla.Models.Components;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Models.DTOs.Usuarios;
using SGPla.Models.ViewModels.ProgramasEducativos;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;


namespace SGPla.Controllers
{
    public class ProgramasEducativosController : Controller
    {
        private readonly IProgramaEducativoService _programaEducativoService;
        private readonly IEntidadAcademicaRepository _entidadAcademicaRepository;
        private readonly ILogger<ProgramasEducativosController> _logger;

        public ProgramasEducativosController(IProgramaEducativoService programaEducativoService, IEntidadAcademicaRepository entidadAcademicaRepository, ILogger<ProgramasEducativosController> logger)
        {
            _programaEducativoService = programaEducativoService;
            _entidadAcademicaRepository = entidadAcademicaRepository;
            _logger = logger;
        }

        // GET: Programas Educativos
        public async Task<IActionResult> Index(string? busqueda, string? region, int? idAreaAcademica, int? idEntidadAcademica, int pagina =1, int cantidad = 10)
        {
            // combos
            var regionesCombo = Constantes.Regiones
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r,
                    Selected = r == region
                })
                .ToList();

            var areas = await _programaEducativoService.ObtenerOpcionesAreaAcademicaAsync();

            var areasCombo = areas
                .Select(a => new OptionModel
                {
                    Value = a.IdAreaAcademica.ToString(),
                    Text = a.Nombre,
                    Selected = idAreaAcademica.HasValue &&
                               a.IdAreaAcademica == idAreaAcademica.Value
                })
                .ToList();
            List<EntidadAcademica> entidades;

            if (idAreaAcademica.HasValue)
            {
                entidades = await _programaEducativoService
                    .ObtenerOpcionesEntidadAcademicaAsync(region, idAreaAcademica.Value);
            }
            else
            {
                entidades = new List<EntidadAcademica>();
                idEntidadAcademica = null;
            }

            var entidadesCombo = entidades
                .Select(e => new OptionModel
                {
                    Value = e.IdEntidadAcademica.ToString(),
                    Text = e.Nombre,
                    Selected = idEntidadAcademica.HasValue &&
                               e.IdEntidadAcademica == idEntidadAcademica.Value
                })
                .ToList();

            return View(new IndexViewModel
            {
                Table = await LlenarTabla(busqueda, region, idAreaAcademica, idEntidadAcademica, pagina, cantidad),
                Regiones = regionesCombo,
                Areas = areasCombo,
                Entidades = entidadesCombo,

                RegionSeleccionada = region,
                IdAreaSeleccionada = idAreaAcademica,
                IdEntidadSeleccionada = idEntidadAcademica,
                Busqueda = busqueda,
                PaginaActual = pagina,
                CantidadPorPagina = cantidad
            });

        }

        private async Task<TableModel> LlenarTabla(string? busqueda, string? region, int? idAreaAcademica, int? idEntidadAcademica, int pagina =1, int cantidad = 10)
        {
            try
            {
                BuscarProgramaEducativoDTO filtros = new BuscarProgramaEducativoDTO
                {
                    Nombre = busqueda,
                    Region = region,
                    IdAreaAcademica = idAreaAcademica,
                    IdEntidadAcademica = idEntidadAcademica,
                    Pagina = pagina,
                    Cantidad = cantidad
                };
                var resultado = await _programaEducativoService.BuscarPorFiltroPaginadoAsync(filtros);

                if (resultado.Items == null || !resultado.Items.Any())
                {
                    return new TableModel
                    {
                        Headers = new List<string> { "Código", "Año", "Periodo", "Acciones" },
                        Rows = new List<TableRowModel>(),
                        Pagination = new PaginationInfo
                        {
                            CurrentPage = pagina,
                            PageSize = cantidad,
                            TotalItems = 0,
                            OnPageChange = "cambiarPaginaPeriodos"
                        }
                    };
                }


                return new TableModel
                {
                    Headers = new List<string>
                        {
                            "Nombre", "Región", "Área Académica", "Entidad Académica", "Acciones"
                        },
                    Rows = resultado.Items.Select(programa => new TableRowModel
                    {
                        Cells = new List<TableCellModel>
                        {
                            new() { Value = programa.Nombre },
                            new() { Value = programa.Region },
                            new() { Value = programa.AreaAcademica },
                            new() { Value = programa.EntidadAcademica },
                            new()
                        {
                        Actions = new List<TableActionModel>
                        {
                            new()
                            {
                                Accion = "ver",
                                Url = Url.Action("VerProgramaEducativo", "ProgramasEducativos", new { id = programa.IdProgramaEducativo })
                            },
                            new()
                            {
                                Accion = "editar",
                                Url = Url.Action("EditarProgramaEducativo", "ProgramasEducativos", new { id = programa.IdProgramaEducativo })
                            },
                            new()
                            {
                                Accion = "eliminar",
                                OnClick = $"abrirModalConfirmacion('¿Desea eliminar este programa educativo?', function() {{ eliminarProgramaEducativo({programa.IdProgramaEducativo}); }})"
                            }
                        }
                    },

                }
                    }).ToList(),
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = pagina,
                        PageSize = cantidad,
                        TotalItems = resultado.TotalCount,
                        OnPageChange = "cambiarPagina"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de programas educativos");
                TempData["Error"] = "Error al cargar los programas educativos";

                return new TableModel();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearProgramaEducativo(CrearProgramaEducativoDTO dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var resultado = await _programaEducativoService.CrearAsync(dto);
                    TempData["Success"] = $"Programa educativo creado correctamente";
                }
                catch (ArgumentException ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));

        }

        //GET: ProgramasEducativos/CrearProgramaEducativo
        public async Task<IActionResult> CrearProgramaEducativo(string? region, int? idAreaAcademica, int? idEntidadAcademica)
        {
            return View(await ObtenerModelo(region, idAreaAcademica, idEntidadAcademica));
        }

        private async Task<CrearProgramaEducativoViewModel> ObtenerModelo(string? region, int? idAreaAcademica, int? idEntidadAcademica)
        {


            var regionesCombo = Constantes.Regiones
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r,
                    Selected = r == region
                })
                .ToList();

            var areas = await _programaEducativoService.ObtenerOpcionesAreaAcademicaAsync();

            var areasCombo = areas
                .Select(a => new OptionModel
                {
                    Value = a.IdAreaAcademica.ToString(),
                    Text = a.Nombre,
                    Selected = idAreaAcademica.HasValue &&
                               a.IdAreaAcademica == idAreaAcademica.Value
                })
                .ToList();

            List<EntidadAcademica> entidades;

            if (idAreaAcademica.HasValue)
            {
                entidades = await _entidadAcademicaRepository
                    .ObtenerPorIdAreaAcademicaAsync(idAreaAcademica.Value);
            }
            else
            {
                entidades = new List<EntidadAcademica>();
                idEntidadAcademica = null;
            }

            var entidadesCombo = entidades
                .Select(e => new OptionModel
                {
                    Value = e.IdEntidadAcademica.ToString(),
                    Text = e.Nombre,
                    Selected = idEntidadAcademica.HasValue &&
                               e.IdEntidadAcademica == idEntidadAcademica.Value
                })
                .ToList();
            return (new CrearProgramaEducativoViewModel
            {
                Regiones = regionesCombo,
                Areas = areasCombo,
                Entidades = entidadesCombo,
            });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEntidades(string region, int idArea)
        {
            var entidades = await _programaEducativoService
        .ObtenerOpcionesEntidadAcademicaAsync(region, idArea);

            var result = entidades.Select(e => new
            {
                value = e.IdEntidadAcademica,
                text = e.Nombre
            });

            return Json(result);
        }


        [HttpGet]
        public async Task<IActionResult> Buscar(BuscarProgramaEducativoDTO filtro)
        {
            try
            {
                var resultados = await _programaEducativoService.BuscarPorFiltroAsync(filtro);

                ViewBag.AreasAcademicas = await _programaEducativoService.ObtenerOpcionesAreaAcademicaAsync();

                return View("Index", resultados);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> EditarProgramaEducativo(int id)
        {
            var programaEducativo = await _programaEducativoService.ObtenerPorIdAsync(id);

            var model = new CrearProgramaEducativoViewModel
            {
                IdProgramaEducativo = id,
                Nombre = programaEducativo.Nombre,
                Campus = programaEducativo.Campus,
                IdEntidadAcademica = programaEducativo.IdEntidadAcademica,
                IdAreaAcademica = programaEducativo.IdAreaAcademica,
                Region = programaEducativo.Region,

            };

            await CargarCombos(model);

            return View(model);
        }

        private async Task CargarCombos(CrearProgramaEducativoViewModel model)
        {


            model.Regiones = Constantes.Regiones
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r,
                    Selected = r == model.Region
                }).ToList();


            var areas = await _programaEducativoService.ObtenerOpcionesAreaAcademicaAsync();

            model.Areas = areas.Select(a => new OptionModel
            {
                Value = a.IdAreaAcademica.ToString(),
                Text = a.Nombre,
                Selected = model.IdAreaAcademica.HasValue &&
                           a.IdAreaAcademica == model.IdAreaAcademica.Value
            }).ToList();


            if (model.IdAreaAcademica.HasValue && !string.IsNullOrEmpty(model.Region))
            {
                var entidades = await _entidadAcademicaRepository
                    .ObtenerPorIdAreaAcademicaYRegionAsync(model.IdAreaAcademica.Value, model.Region);

                model.Entidades = entidades.Select(e => new OptionModel
                {
                    Value = e.IdEntidadAcademica.ToString(),
                    Text = e.Nombre,
                    Selected = model.IdEntidadAcademica > 0 &&
                               e.IdEntidadAcademica == model.IdEntidadAcademica
                }).ToList();
            }
            else
            {
                model.Entidades = new List<OptionModel>();
            }
        }

        public async Task<IActionResult> VerProgramaEducativo(int id)
        {

            if (id == 0)
            {
                return BadRequest();
            }

            try
            {


                var programaEducativo = await _programaEducativoService.ObtenerPorIdAsync(id);

                if (programaEducativo == null)
                {
                    return NotFound();
                }

                return View(programaEducativo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles del programa educativo", id);
                TempData["Error"] = "Error al cargar los detalles del programa educativo";
                return RedirectToAction(nameof(Index));
            }
        }


                [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarProgramaEducativo(EditarProgramaEducativoDTO dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var resultado = await _programaEducativoService.EditarAsync(dto);
                    TempData["Success"] = $"Programa educativo actualizado correctamente";
                }
                catch (ArgumentException ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProgramaEducativo(int id)
        {
            try
            {
                var resultado = await _programaEducativoService.EliminarAsync(id);
                if (resultado)
                    TempData["Success"] = "Programa educativo eliminado exitosamente";
                else
                    TempData["Error"] = $"Error al eliminar el programa educativo";
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error al eliminar el programa educativo {Id}", id);
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));

        }



    }
}
