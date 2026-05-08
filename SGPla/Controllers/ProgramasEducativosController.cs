using Microsoft.AspNetCore.Mvc;
<<<<<<< Updated upstream
using SGPla.Models.DTOs.AreaAcademica;
=======
using SGPla.Commons;
using SGPla.Models;
using SGPla.Models.Components;
>>>>>>> Stashed changes
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Models.ViewModels.ProgramasEducativos;
using SGPla.Services.Interfaces;


namespace SGPla.Controllers
{
    public class ProgramasEducativosController : Controller
    {
        private readonly IProgramaEducativoService _programaEducativoService;
        private readonly ILogger<ProgramasEducativosController> _logger;

        public ProgramasEducativosController(IProgramaEducativoService programaEducativoService, ILogger<ProgramasEducativosController> logger)
        {
            _programaEducativoService = programaEducativoService;
            _logger = logger;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index(string? busqueda, string? region, int? idAreaAcademica, int? idEntidadAcademica)
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
                Table = await LlenarTabla(busqueda, region, idAreaAcademica, idEntidadAcademica),
                Regiones = regionesCombo,
                Areas = areasCombo,
                Entidades = entidadesCombo,

                RegionSeleccionada = region,
                IdAreaSeleccionada = idAreaAcademica,
                IdEntidadSeleccionada = idEntidadAcademica,
                Busqueda = busqueda
            });

        }

        private async Task<TableModel> LlenarTabla(string? busqueda, string? region, int? idAreaAcademica, int? idEntidadAcademica)
        {
            try
            {
                BuscarProgramaEducativoDTO filtros = new BuscarProgramaEducativoDTO
                {
                    Nombre = busqueda,
                    Region = region,
                    IdAreaAcademica = idAreaAcademica,
                    IdEntidadAcademica = idEntidadAcademica
                };
                var programas = await _programaEducativoService.BuscarPorFiltroAsync(filtros);

                return new TableModel
                {
                    Headers = new List<string>
                        {
                            "Nombre", "Región", "Área Académica", "Entidad Académica", "Acciones"
                        },
                    Rows = programas.Select(programa => new TableRowModel
                    {
                        Cells = new List<TableCellModel>
                        {
                            new() { Value = programa.Nombre },
                            new() { Value = programa.Region },
                            new() { Value = programa.AreaAcademica },
                            new() { Value = programa.EntidadAcademica },
                            new()
                        {
                        Value = !string.IsNullOrEmpty(programa.EntidadAcademica)
                            ? programa.EntidadAcademica
                            : programa.AreaAcademica
                    },
                    new() { Value = programa.Region },
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
                    }
                }
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de usuarios");
                TempData["Error"] = "Error al cargar los usuarios";

                return new TableModel();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearProgramaEducativoDTO dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    dto.IdEntidadAcademica = 100;
                    var resultado = await _programaEducativoService.CrearAsync(dto);
                    TempData["Success"] = $"Programa Educativo creado exitosamente con ID: {resultado.IdProgramaEducativo}";
                }
                catch (ArgumentException ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));

        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEntidadesAcademicas(string region, int idArea)
        {
            var entidades = await _programaEducativoService
        .ObtenerOpcionesEntidadAcademicaAsync(region, idArea);

            return Json(entidades);
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarProgramaEducativoDTO dto)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    var resultado = await _programaEducativoService.EditarAsync(dto);
                    TempData["Success"] = $"Programa Educativo editado exitosamente con ID: {resultado.IdProgramaEducativo}";
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
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var resultado = await _programaEducativoService.EliminarAsync(id);
                if (resultado)
                    TempData["Success"] = $"Programa Educativo eliminado exitosamente con ID: {id}";
                else
                    TempData["Error"] = $"No se pudo eliminar el Programa Educativo con ID: {id}";
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));

        }


       
    }
}
