using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SGPla.Commons;
using SGPla.Models;
using SGPla.Models.Components;
using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Models.ViewModels.PlanesEstudios;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers
{
    public class PlanesEstudiosController : Controller
    {
        private readonly IPlanEstudiosService _planEstudiosService;
        private readonly IAreaAcademicaService _areaAcademicaService;
        private readonly IEntidadAcademicaService _entidadAcademicaService;
        private readonly IProgramaEducativoService _programaEducativoService;
        private readonly ILogger<PlanesEstudiosController> _logger;

        public PlanesEstudiosController(
            IPlanEstudiosService planEstudiosService,
            IAreaAcademicaService areaAcademicaService,
            IEntidadAcademicaService entidadAcademicaService,
            IProgramaEducativoService programaEducativoService,
            ILogger<PlanesEstudiosController> logger)
        {
            _planEstudiosService = planEstudiosService;
            _areaAcademicaService = areaAcademicaService;
            _entidadAcademicaService = entidadAcademicaService;
            _programaEducativoService = programaEducativoService;
            _logger = logger;
        }

        //GET: Usuarios
        public async Task<IActionResult> Index(string? busqueda, string? region, int? idAreaAcademica, int? idEntidadAcademica, int? idProgramaEducativo)
        {
            var regionesCombo = generarCatalogoRegiones(region);
            var areasCombo = new List<OptionModel>();
            var entidadesCombo = new List<OptionModel>();
            var programasCombo = new List<OptionModel>();

            //Llenar Areas
            if (!region.IsNullOrEmpty())
                areasCombo = await generarCatalogoAreasAsync(idAreaAcademica);
            else
                idAreaAcademica = null;

            //Llenar Entidades
            if (idAreaAcademica.HasValue && !region.IsNullOrEmpty())
                entidadesCombo = await generarCatalogoEntidadesAsync(idAreaAcademica.Value, region, idEntidadAcademica);
            else
            {
                idEntidadAcademica = null;
            }

            //Llenar Programas Educativos
            if (idEntidadAcademica.HasValue && idAreaAcademica.HasValue)
                programasCombo = await generarCatalogoProgramasAsync(idAreaAcademica.Value, region, idEntidadAcademica.Value, idProgramaEducativo);
            else
            {
                idProgramaEducativo = null;
            }

            return View(new IndexViewModel
            {
                Table = await LlenarTabla(busqueda, region, idAreaAcademica, idEntidadAcademica, idProgramaEducativo),
                Regiones = regionesCombo,
                Areas = areasCombo,
                Entidades = entidadesCombo,
                ProgramasEducativos = programasCombo,

                RegionSeleccionada = region,
                IdAreaSeleccionada = idAreaAcademica,
                IdEntidadSeleccionada = idEntidadAcademica,
                IdProgramaSeleccionado = idProgramaEducativo,
                Busqueda = busqueda
            });
        }

        private List<OptionModel> generarCatalogoRegiones(string region)
        {
            return Constantes.Regiones
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r,
                    Selected = r == region
                })
                .ToList();
        }

        private async Task<List<OptionModel>> generarCatalogoAreasAsync(int? idAreaAcademica)
        {
            var areas = await _areaAcademicaService.ObtenerTodasAsync();

            return areas.Select(a => new OptionModel
                {
                    Value = a.IdAreaAcademica.ToString(),
                    Text = a.Nombre,
                    Selected = idAreaAcademica.HasValue &&
                               a.IdAreaAcademica == idAreaAcademica.Value
                })
                .ToList();
        }

        private async Task<List<OptionModel>> generarCatalogoEntidadesAsync(int idAreaAcademica, string region, int? idEntidadAcademica)
        {
            var filtros = new FiltroEntidadAcademicaDTO
            {
                IdAreaAcademica = idAreaAcademica,
                Region = region,
                
            };

            var entidades = await _entidadAcademicaService.ObtenerPorFiltroAsync(filtros, 1);

            return entidades.Select(e => new OptionModel
            {
                Value = e.IdEntidadAcademica.ToString(),
                Text = e.Nombre,
                Selected = idEntidadAcademica.HasValue &&
                    e.IdEntidadAcademica == idEntidadAcademica
            }).ToList();
        }

        private async Task<List<OptionModel>> generarCatalogoProgramasAsync(int idAreaAcademica, string region, int idEntidadAcademica, int? idProgramaEducativo)
        {
            var filtros = new BuscarProgramaEducativoDTO
            {
                Region = region,
                IdAreaAcademica = idAreaAcademica,
                IdEntidadAcademica = idEntidadAcademica,
            };

            var programas = await _programaEducativoService.BuscarPorFiltroAsync(filtros);

            return programas.Select(p => new OptionModel
            {
                Value = p.IdProgramaEducativo.ToString(),
                Text = p.Nombre,
                Selected = idProgramaEducativo.HasValue &&
                    p.IdEntidadAcademica == idProgramaEducativo
            }).ToList();
        }

        private async Task<TableModel> LlenarTabla(string? busqueda, string? region, int? idAreaAcademica, int? idEntidadAcademica, int? idProgramaEducativo)
        {
            try
            {
                int idEntidad = 0;
                int idPrograma = 0;
                if (idEntidadAcademica.HasValue)
                    idEntidad = idEntidadAcademica.Value;
                if (idProgramaEducativo.HasValue)
                    idPrograma = idProgramaEducativo.Value;

                FiltroPlanEstudiosDTO filtros = new FiltroPlanEstudiosDTO
                {
                    IdEntidadAcademica = idEntidad,
                    IdProgramaEducativo = idPrograma,
                    Nombre = busqueda
                };
                var planes = await _planEstudiosService.ObtenerPorFiltroAsync(filtros, 1);

                return new TableModel
                {
                    Headers = new List<string>
                        {
                            "Programa Educativo", "Modalidad", "Plan", "Área",  "Acciones"
                        },
                    Rows = planes.Select(plan => new TableRowModel
                    {
                        Cells = new List<TableCellModel>
                        {
                            new() { Value = plan.NombreProgramaEducativo },
                            new() { Value = plan.Modalidad },
                            new() { Value = plan.Nombre },
                            new() { Value = plan.Area },
                            new()
                            {
                                Actions = new List<TableActionModel>
                                {
                                    new()
                                    {
                                        Accion = "ver",
                                        Url = Url.Action("VerPlanEstudios", "PlanesEstudios", new { id = plan.IdPlanEstudios})
                                    },
                                    new()
                                    {
                                        Accion = "editar",
                                        //Url = Url.Action("EditarUsuario", "Usuarios", new { id = plan.IdUsuario, rol = plan.Rol })
                                    },
                                    new()
                                    {
                                        Accion = "eliminar",
                                        //Url = Url.Action("Delete", "Usuarios", new { id = plan.IdUsuario, rol = plan.Rol })
                                    }
                                }
                            }
                        }
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de Planes de Estudios.");
                TempData["Error"] = "Error al cargar los Planes de Estudios.";

                return new TableModel();
            }
        }
    }
}
