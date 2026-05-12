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
using System.Numerics;

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

        //Menú
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
                Table = await LlenarTablaIndex(busqueda, region, idAreaAcademica, idEntidadAcademica, idProgramaEducativo),
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

        private async Task<TableModel> LlenarTablaIndex(string? busqueda, string? region, int? idAreaAcademica, int? idEntidadAcademica, int? idProgramaEducativo)
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

        //Ver Plan de Estudios
        public async Task<IActionResult> VerPlanEstudios(int id)
        {
            if (id == 0)
                return BadRequest();

            try
            {
                var plan = await _planEstudiosService.ObtenerPorIdAsync(id);
                if (plan == null)
                    return NotFound();

                return View(new VerPlanEstudiosViewModel
                {
                    IdPlanEstudios = plan.IdPlanEstudios,
                    NombreProgramaEducativo = plan.NombreProgramaEducativo,
                    Modalidad = plan.Modalidad,
                    Nombre = plan.Nombre,
                    NombreAreaAcademica = plan.NombreAreaAcademica,
                    ExperienciasEducativas = plan.ExperienciasEducativas,
                    Table = LlenarTablaVerPlanEstudios(plan.ExperienciasEducativas)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles del Plan de Estudios {Id}", id);
                TempData["Error"] = "Error al cargar los detalles del Plan de Estudios";
                return RedirectToAction(nameof(Index));
            }
        }

        private TableModel LlenarTablaVerPlanEstudios(List<DatosExperienciaEducativaDTO> experiencias)
        {
            return new TableModel
            {
                Headers = new List<string> { "Codigo", "Experiencia Educativa", "Perfil Docente" },
                Rows = experiencias.Select(ee => new TableRowModel
                {
                    Cells = new List<TableCellModel>
                    {
                        new() { Value = ee.Codigo },
                        new() { Value = ee.Nombre },
                        new()
                        {
                            Actions = new List<TableActionModel>
                            {
                                new()
                                {
                                    Accion = "informacion",
                                    OnClick = $"abrirModalPerfilDocente(\"{ee.PerfilDocente}\")"
                                }
                            }
                        }
                    }
                }).ToList()
            };
        }

        //Paso 1
        public async Task<IActionResult> CargarPlanPaso1(CargarPlanPaso1ViewModel modelo)
        {
            var region = modelo.Region;
            var plan = modelo.Plan;
            var modalidad = modelo.Sistema;
            var idAreaAcademica = modelo.Area;
            var idEntidadAcademica = modelo.Entidad;
            var idProgramaEducativo = modelo.Programa;

            var regionesCombo = generarCatalogoRegiones(region);
            var areasCombo = new List<OptionModel>();
            var entidadesCombo = new List<OptionModel>();
            var programasCombo = new List<OptionModel>();
            var nombre = plan;
            var modalidadesCombo = generarCatalogoModalidades(modalidad);

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

            return View(new CargarPlanPaso1ViewModel
            {
                Region = region,
                Area = idAreaAcademica,
                Entidad = idEntidadAcademica,
                Programa = idProgramaEducativo,
                Plan = nombre,
                Archivo = modelo.Archivo,

                ListaRegiones = regionesCombo,
                ListaAreas = areasCombo,
                ListaEntidades = entidadesCombo,
                ListaProgramas = programasCombo,
                ListaSistema = modalidadesCombo
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

        [HttpGet]
        public async Task<JsonResult> ObtenerAreasAsync()
        {
            var areas =
                await generarCatalogoAreasAsync(null);

            return Json(areas);
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

        [HttpGet]
        public async Task<JsonResult> ObtenerEntidadesAsync(int idAreaAcademica, string region)
        {
            var entidades =
                await generarCatalogoEntidadesAsync(idAreaAcademica, region, 1);

            return Json(entidades);
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

        [HttpGet]
        public async Task<JsonResult> ObtenerProgramasAsync(int idAreaAcademica, string region, int idEntidadAcademica)
        {
            var entidades =
                await generarCatalogoProgramasAsync(idAreaAcademica, region, idEntidadAcademica, null);

            return Json(entidades);
        }

        private List<OptionModel> generarCatalogoModalidades(string? modalidad)
        {
            return Constantes.Modalidades
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r,
                    Selected = r == modalidad
                })
                .ToList();
        }

        //Procesar Archivo
        public async Task<List<DatosExperienciaEducativaDTO>> ProcesarArchivoAsync(ArchivoPlanEstudiosDTO archivo)
        {
            try
            {
                var listaEe = _planEstudiosService.ProcesarArchivo(archivo);
                return listaEe;
            }
            catch (Exception ex)
            {
                return [];
            }
        }

        //Paso 2
        public async Task<IActionResult> CargarPlanPaso2(CargarPlanPaso1ViewModel modelo)
        {
            var archivo = modelo.Archivo;
            var archivoDto = new ArchivoPlanEstudiosDTO
            {
                Archivo = archivo.OpenReadStream(),
                NombreArchivo = archivo.FileName
            };
            var listaEe = await ProcesarArchivoAsync(archivoDto);

            var area = await _areaAcademicaService.ObtenerPorIdAsync((int)modelo.Area);
            var programa = await _programaEducativoService.ObtenerPorIdAsync((int)modelo.Programa);

            return View(new CargarPlanPaso2ViewModel
            {
                Region = modelo.Region,
                Area = area.Nombre,
                ProgramaEducativo = programa.Nombre,
                IdProgramaEducativo = (int)modelo.Programa,
                Plan = modelo.Plan,
                Sistema = modelo.Sistema,
                Table = LlenarTablaCargarPaso2(listaEe),
                Archivo = archivoDto,
                Experiencias = listaEe
            });
        }

        private TableModel LlenarTablaCargarPaso2(List<DatosExperienciaEducativaDTO> experiencias)
        {
            return new TableModel
            {
                Headers = new List<string> { "Codigo", "Experiencia Educativa", "Perfil Docente", "Acciones" },
                Rows = experiencias.Select(ee => new TableRowModel
                {
                    Cells = new List<TableCellModel>
                    {
                        new() { Value = ee.Codigo },
                        new() { Value = ee.Nombre },
                        new()
                        {
                            Actions = new List<TableActionModel>
                            {
                                new()
                                {
                                    Accion = "informacion",
                                    OnClick = $"abrirModalPerfilDocente(\"{ee.PerfilDocente}\")"
                                }
                            }
                        },
                        new()
                            {
                                Actions = new List<TableActionModel>
                                {
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

        [HttpPost]
        public async Task<IActionResult> GuardarPlanEstudiosAsync(CargarPlanPaso2ViewModel modelo)
        {
            try
            {
                await _planEstudiosService.AgregarAsync(new CrearPlanEstudiosDTO
                {
                    IdProgramaEducativo = (int)modelo.IdProgramaEducativo,
                    Nombre = modelo.Plan,
                    Sistema = modelo.Sistema,
                    Archivo = modelo.Archivo,
                    ExperienciasEducativas = modelo.Experiencias.Select(ee => new AgregarExperienciaEducativaDTO
                    {
                        Codigo = ee.Codigo,
                        Nombre = ee.Nombre,
                        PerfilDocente = ee.PerfilDocente
                    }).ToList()
                });
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar el Plan de Estudios.");
                TempData["Error"] = "Error al guardar el Plan de Estudios.";
                return RedirectToAction(nameof(CargarPlanPaso1), modelo);
            }
        }
    }
}
