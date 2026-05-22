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
using System.Runtime.ConstrainedExecution;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace SGPla.Controllers
{
    public class PlanesEstudiosController : Controller
    {
        private readonly IPlanEstudiosService _planEstudiosService;
        private readonly IAreaAcademicaService _areaAcademicaService;
        private readonly IEntidadAcademicaService _entidadAcademicaService;
        private readonly IProgramaEducativoService _programaEducativoService;
        private readonly IArchivoService _archivoService;
        private readonly ILogger<PlanesEstudiosController> _logger;
        private readonly IWebHostEnvironment _environment;
        private const string NOMBRE_LOGGER = "FRONT-PLANES:";
        private int paginaActual = 1;

        public PlanesEstudiosController(
            IPlanEstudiosService planEstudiosService,
            IAreaAcademicaService areaAcademicaService,
            IEntidadAcademicaService entidadAcademicaService,
            IProgramaEducativoService programaEducativoService,
            IArchivoService archivoService,
            ILogger<PlanesEstudiosController> logger,
            IWebHostEnvironment environment)
        {
            _planEstudiosService = planEstudiosService;
            _areaAcademicaService = areaAcademicaService;
            _entidadAcademicaService = entidadAcademicaService;
            _programaEducativoService = programaEducativoService;
            _archivoService = archivoService;
            _logger = logger;
            _environment = environment;
        }

        // ==========
        // INDEX
        // ==========

        //Vista
        [HttpGet]
        public async Task<IActionResult> Index(string? busqueda, string? region, int? idAreaAcademica, int? idEntidadAcademica, int? idProgramaEducativo, int pagina = 1, int cantidad = 10)
        {
            HttpContext.Session.Clear();

            var modelo = new IndexViewModel
            {
                Table = generarTablaError(),
                Regiones = new List<OptionModel>(),
                Areas = new List<OptionModel>(),
                Entidades = new List<OptionModel>(),
                ProgramasEducativos = new List<OptionModel>()
            };

            paginaActual = pagina;

            var regionesCombo = generarCatalogoRegiones();
            var areasCombo = new List<OptionModel>();
            var entidadesCombo = new List<OptionModel>();
            var programasCombo = new List<OptionModel>();

            try
            {
                

                if (!region.IsNullOrEmpty())
                    areasCombo = await generarCatalogoAreasAsync(idAreaAcademica);
                else
                    idAreaAcademica = null;

                if (idAreaAcademica.HasValue)
                    entidadesCombo = await generarCatalogoEntidadesAsync(idAreaAcademica.Value, region, idEntidadAcademica);
                else
                    idEntidadAcademica = null;

                if (idEntidadAcademica.HasValue)
                    programasCombo = await generarCatalogoProgramasAsync(idAreaAcademica.Value, region, idEntidadAcademica.Value, idProgramaEducativo);
                else
                    idProgramaEducativo = null;

                modelo.Table = await LlenarTablaIndexAsync(busqueda, region, idAreaAcademica, idEntidadAcademica, idProgramaEducativo, pagina, cantidad);
                modelo.Regiones = regionesCombo;
                modelo.Areas = areasCombo;
                modelo.Entidades = entidadesCombo;
                modelo.ProgramasEducativos = programasCombo;
                modelo.PaginaActual = paginaActual;
                modelo.CantidadPorPaginas = cantidad;

                return View(modelo);
            }
            catch (ValidacionExcepction vx)
            {
                _logger.LogError(vx, "{NOMBRE_LOGGER} Error al cargar los catálogos.", NOMBRE_LOGGER);
                TempData["Error"] = "Ha ocurrido un error al cargar los catálogos.";
                return View(modelo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{NOMBRE_LOGGER} [Index] Error inesperado.", NOMBRE_LOGGER);
                TempData["Error"] = "Ha ocurrido un error, inténtalo de nuevo más tarde.";
                return View(modelo);
            }
        }

        private async Task<TableModel> LlenarTablaIndexAsync(string? busqueda, string? region, int? idAreaAcademica, int? idEntidadAcademica, int? idProgramaEducativo, int pagina = 1, int cantidad = 10)
        {
            _logger.LogInformation("{NOMBRE_LOGGER} Obteniendo tabla de Planes de Estudios con filtros:\n" +
                " - Búsqueda: {busqueda}\n - Región: {region}\n - idAreaAcademica: {idAreaAcademica}\n - idEntidadAcademica: {idEntidadAcademica}\n" +
                " - idProgramaEducativo: {idProgramaEducativo}",
                NOMBRE_LOGGER, busqueda, region, idAreaAcademica, idEntidadAcademica, idProgramaEducativo);
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
                var planes = await _planEstudiosService.ObtenerPorFiltroAsync(filtros, cantidad, pagina);
                if (planes.items.Count == 0)
                {
                    paginaActual = 1;
                    planes = await _planEstudiosService.ObtenerPorFiltroAsync(filtros, cantidad, paginaActual);
                }

                return new TableModel
                {
                    Headers = new List<string>
                    {
                        "Programa Educativo", "Modalidad", "Plan", "Área",  "Acciones"
                    },
                    Rows = planes.items.Select(plan => new TableRowModel
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
                                        Url = Url.Action("VerPlanEstudios", "PlanesEstudios", new { idPlanEstudios = plan.IdPlanEstudios})
                                    },
                                    new()
                                    {
                                        Accion = "editar",
                                        Url = Url.Action("EditarPlan", "PlanesEstudios", new { idPlanEstudios = plan.IdPlanEstudios, recarga = false})
                                    },
                                    new()
                                    {
                                        Accion = "eliminar",
                                        OnClick = $"abrirModalEliminarPlan({plan.IdPlanEstudios})"
                                    }
                                }
                            }
                        }
                    }).ToList(),
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = paginaActual,
                        PageSize = cantidad,
                        TotalItems = planes.cantidad,
                        OnPageChange = "cambiarPagina"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{NOMBRE_LOGGER} Error al obtener la lista de Planes de Estudios.");

                return generarTablaError();
            }
        }

        [HttpGet]
        public async Task EliminarPlanEstudiosAsync(int idPlanEstudios)
        {
            try
            {
                await _planEstudiosService.EliminarAsync(idPlanEstudios);
                TempData["Success"] = "El elemento ha sido eliminado con éxito.";

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{NOMBRE_LOGGER} Error al eliminar el Plan de Estudios.", NOMBRE_LOGGER);
                TempData["Error"] = "Error al eliminar el Plan de Estudios.";
            }
        }

        // ==========
        // VER
        // ==========

        //Vista
        [HttpGet]
        public async Task<IActionResult> VerPlanEstudiosAsync(int idPlanEstudios, int pagina = 1, int cantidad = 10)
        {
            _logger.LogInformation("{NOMBRE_LOGGER} Visualizando Plan de Estudios con Id: {id}", NOMBRE_LOGGER, idPlanEstudios);

            paginaActual = pagina;
            var modelo = new VerPlanEstudiosViewModel();
            modelo.Table = generarTablaError();

            try
            {
                var plan = await _planEstudiosService.ObtenerPorIdAsync(idPlanEstudios);
                modelo.IdPlanEstudios = plan.IdPlanEstudios;
                modelo.NombreProgramaEducativo = plan.NombreProgramaEducativo;
                modelo.Modalidad = plan.Modalidad;
                modelo.Nombre = plan.Nombre;
                modelo.NombreAreaAcademica = plan.NombreAreaAcademica;
                modelo.IdArchivo = plan.IdArchivo;
                modelo.Table = LlenarTablaVerPlanEstudios(plan.ExperienciasEducativas, paginaActual, cantidad);
                modelo.PaginaActual = paginaActual;
                modelo.CantidadPorPaginas = cantidad;
                return View(modelo);
            }
            catch (ValidacionExcepction vx)
            {
                _logger.LogError(vx, "{NOMBRE_LOGGER} Error al obtener detalles del Plan de Estudios: {id}", NOMBRE_LOGGER, idPlanEstudios);
                TempData["Error"] = $"No se ha podido mostrar el Plan de Estudios.";
                if (vx.Codigo.Contains("404"))
                    TempData["Error"] = $"No se ha encontrado al Plan de Estudios.";

                return View(modelo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{NOMBRE_LOGGER} [VerPlanEstudios] Error inesperado.", NOMBRE_LOGGER);
                TempData["Error"] = "Ha ocurrido un error, inténtalo de nuevo más tarde.";
                return View(modelo);
            }
        }

        private TableModel LlenarTablaVerPlanEstudios(List<DatosExperienciaEducativaDTO> experiencias, int pagina = 1, int cantidad = 10)
        {
            _logger.LogInformation("{NOMBRE_LOGGER} Generando tabla con Experiencias Educativas.", NOMBRE_LOGGER);
            try
            {
                int skip = (pagina - 1) * cantidad;
                var experienciasTabla = experiencias.Skip(skip).Take(cantidad).ToList();

                return new TableModel
                {
                    Headers = new List<string> { "Codigo", "Experiencia Educativa", "Perfil Docente" },
                    Rows = experienciasTabla.Select(ee => new TableRowModel
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
                    }).ToList(),
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = paginaActual,
                        PageSize = cantidad,
                        TotalItems = experiencias.Count(),
                        OnPageChange = "cambiarPagina"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{NOMBRE_LOGGER} Error al generar la tabla de Experiencias Educativas.", NOMBRE_LOGGER);

                return generarTablaError();
            }
        }

        [HttpGet]
        public async Task<IActionResult> DescargarArchivoAsync(int idArchivo)
        {
            try
            {
                var archivoDto = await _archivoService.DescargarAsync(idArchivo);
                if (archivoDto != null)
                {
                    var rutaCompleta = Path.Combine("Archivos/planes-estudios", archivoDto.Ruta);
                    _logger.LogInformation("Ruta: {rutaCompleta}", rutaCompleta);

                    if (System.IO.File.Exists(rutaCompleta))
                    {
                        var stream = new FileStream(rutaCompleta, FileMode.Open, FileAccess.Read);
                        return File(stream, archivoDto.Tipo, archivoDto.Nombre);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch
            {
                return BadRequest();
            }
        }

        // ==========
        // PASO 1
        // ==========

        //Vista
        [HttpGet]
        public async Task<IActionResult> CargarPlanPaso1(string? region, string? plan, string? sistema, int? idAreaAcademica, int? idEntidadAcademica, int? idProgramaEducativo)
        {
            var modelo = new CargarPlanPaso1ViewModel
            {
                ListaRegiones = [],
                ListaAreas = [],
                ListaEntidades = [],
                ListaProgramas = [],
            };

            try
            {
                var regionesCombo = generarCatalogoRegiones();
                var areasCombo = new List<OptionModel>();
                var entidadesCombo = new List<OptionModel>();
                var programasCombo = new List<OptionModel>();
                var nombre = plan;
                var sistemasCombo = generarCatalogoModalidades();

                if (!region.IsNullOrEmpty())
                    areasCombo = await generarCatalogoAreasAsync(idAreaAcademica);
                else
                    idAreaAcademica = null;

                if (idAreaAcademica.HasValue)
                    entidadesCombo = await generarCatalogoEntidadesAsync(idAreaAcademica.Value, region, idEntidadAcademica);
                else
                    idEntidadAcademica = null;

                if (idEntidadAcademica.HasValue)
                    programasCombo = await generarCatalogoProgramasAsync(idAreaAcademica.Value, region, idEntidadAcademica.Value, idProgramaEducativo);
                else
                    idProgramaEducativo = null;

                modelo.Region = region;
                modelo.IdAreaAcademica = idAreaAcademica;
                modelo.IdEntidadAcademica = idEntidadAcademica;
                modelo.IdProgramaEducativo = idProgramaEducativo;
                modelo.Plan = nombre ?? "";
                modelo.Sistema = sistema ?? "";
                modelo.ListaRegiones = regionesCombo;
                modelo.ListaAreas = areasCombo;
                modelo.ListaEntidades = entidadesCombo;
                modelo.ListaProgramas = programasCombo;
                modelo.ListaSistema = sistemasCombo;

                return View(modelo);
            }
            catch (ValidacionExcepction vx)
            {
                _logger.LogError(vx, "{NOMBRE_LOGGER} [CargarPlanPaso1] Error al cargar los catálogos.", NOMBRE_LOGGER);
                TempData["Error"] = "Ha ocurrido un error al cargar los catálogos.";
                modelo.ListaRegiones.Clear();
                return View(modelo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{NOMBRE_LOGGER} [CargarPlanPaso1] Error inesperado.", NOMBRE_LOGGER);
                TempData["Error"] = "Ha ocurrido un error, inténtalo de nuevo más tarde.";
                return View(modelo);
            }

        }

        [HttpPost]
        public async Task<IActionResult> ValidarPaso1Async(CargarPlanPaso1ViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("{NOMBRE_LOGGER} Modelo no válido en Paso 2 de Cargar Plan de Estudios.", NOMBRE_LOGGER);
                if (modelo.Archivo == null)
                    TempData["Error"] = "El archivo es obligatorio.";

                modelo = await recargarCombosAsync(modelo);

                return View(nameof(CargarPlanPaso1), modelo);
            }
            else
            {
                var modeloPaso2 = new CargarPlanPaso2ViewModel
                {
                    Region = modelo.Region ?? "",
                    IdAreaAcademica = modelo.IdAreaAcademica ?? 0,
                    IdEntidadAcademica = modelo.IdEntidadAcademica ?? 0,
                    IdProgramaEducativo = modelo.IdProgramaEducativo ?? 0,
                    Plan = modelo.Plan,
                    Sistema = modelo.Sistema,
                    Archivo = modelo.Archivo,
                    Recarga = false
                };
                return await CargarPlanPaso2(modeloPaso2);
            }
        }

        // ==========
        // PASO 2
        // ==========

        //Vista
        [HttpPost]
        public async Task<IActionResult> CargarPlanPaso2(CargarPlanPaso2ViewModel modelo)
        {
            _logger.LogInformation("{NOMBRE_LOGGER} Paso 2 de Cargar Plan de Estudios.", NOMBRE_LOGGER);

            var vista = new CargarPlanPaso2ViewModel();
            var error = false;

            if (modelo.Recarga)
            {
                var resultado = await recargarPaso2Async(modelo);
                error = resultado.error;
                vista = resultado.modelo;
            }
            else
            {
                var resultado = await inicializarPaso2Async(modelo);
                error = resultado.error;
                vista = resultado.modelo;
            }

            if (error)
            {
                var modeloAnterior = new CargarPlanPaso1ViewModel
                {
                    Region = modelo.Region,
                    IdAreaAcademica = modelo.IdAreaAcademica,
                    IdEntidadAcademica = modelo.IdEntidadAcademica,
                    IdProgramaEducativo = modelo.IdProgramaEducativo,
                    Plan = modelo.Plan,
                    Sistema = modelo.Sistema
                };
                modeloAnterior = await recargarCombosAsync(modeloAnterior);

                return View("CargarPlanPaso1", modeloAnterior);
            }
            else
            {
                return View("CargarPlanPaso2", vista);
            }
        }

        private async Task<(CargarPlanPaso2ViewModel? modelo, bool error)> inicializarPaso2Async(CargarPlanPaso2ViewModel modelo)
        {
            try
            {
                var archivo = modelo.Archivo;
                await guardarArchivoTemporalmente(archivo);

                var ruta = HttpContext.Session.GetString("Ruta");
                if (!string.IsNullOrEmpty(ruta))
                {
                    var archivoDto = new ArchivoPlanEstudiosDTO
                    {
                        Ruta = ruta,
                        NombreArchivo = archivo.FileName
                    };
                    var listaEe = await ProcesarArchivoAsync(archivoDto);

                    HttpContext.Session.SetString("Experiencias", JsonSerializer.Serialize(listaEe));

                    var area = await _areaAcademicaService.ObtenerPorIdAsync(modelo.IdAreaAcademica);
                    var programa = await _programaEducativoService.ObtenerPorIdAsync(modelo.IdProgramaEducativo);

                    modelo.NombreArea = area.Nombre;
                    modelo.NombrePrograma = programa?.Nombre ?? "";
                    modelo.Table = LlenarTablaGestionExperiencias(listaEe);
                    modelo.Table.TableId = "tablaExperiencias";
                    modelo.Table.Pagination = new PaginationInfo
                    {
                        PageSize = 10,
                        TotalItems = listaEe.Count(),
                        PaginationMode = "client"
                    };

                    return (modelo, false);
                }
                else
                {
                    _logger.LogError("{NOMBRE_LOGGER} [CargarPlanPaso2] La ruta del archivo es nula.", NOMBRE_LOGGER);
                    TempData["Error"] = "Error: La ruta del archivo es nula.";

                    return (null, true);
                }
            }
            catch (ValidacionExcepction vx)
            {
                _logger.LogError(vx, "{NOMBRE_LOGGER} [CargarPlanPaso2] Error de validación.", NOMBRE_LOGGER);
                TempData["Error"] = "No se ha podido cargar el Plan de Estudios";
                if (vx.Codigo.Contains("404"))
                    TempData["Error"] = vx.Message;

                return (null, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{NOMBRE_LOGGER} [CargarPlanPaso2] Error inesperado.", NOMBRE_LOGGER);
                TempData["Error"] = "Ha ocurrido un error, inténtalo de nuevo más tarde.";

                return (null, true);
            }
        }

        private async Task<(CargarPlanPaso2ViewModel? modelo, bool error)> recargarPaso2Async(CargarPlanPaso2ViewModel modelo)
        {
            try
            {
                var experienciasJson = HttpContext.Session.GetString("Experiencias");
                if (!string.IsNullOrEmpty(experienciasJson))
                {
                    var experiencias = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(experienciasJson);

                    modelo.Table = LlenarTablaGestionExperiencias(experiencias);
                    modelo.Table.TableId = "tablaExperiencias";
                    modelo.Table.Pagination = new PaginationInfo
                    {
                        PageSize = 10,
                        TotalItems = experiencias.Count(),
                        PaginationMode = "client"
                    };
                    return (modelo, false);

                }
                else
                {
                    _logger.LogError("{NOMBRE_LOGGER} [CargarPlanPaso2] Error al obtener las Experiencias Educativas de la sesión.", NOMBRE_LOGGER);
                    TempData["Error"] = "Error al obtener las Experiencias Educativas de la sesión.";
                    return (null, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{NOMBRE_LOGGER} [CargarPlanPaso2] Error inesperado.", NOMBRE_LOGGER);
                TempData["Error"] = "Ha ocurrido un error, inténtalo de nuevo más tarde.";
                return (null, true);
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegresarPaso1Async([FromBody] CargarPlanPaso1ViewModel modelo)
        {
            var rutaArchivo = HttpContext.Session.GetString("Ruta");
            if (!string.IsNullOrEmpty(rutaArchivo))
            {
                System.IO.File.Delete(rutaArchivo);
                HttpContext.Session.Remove("Ruta");
            }
            //string? region, string? plan, string? modalidad, int? idAreaAcademica, int? idEntidadAcademica, int? idProgramaEducativo
            var url = Url.Action(nameof(CargarPlanPaso1), new
            {
                region = modelo.Region,
                plan = modelo.Plan,
                sistema = modelo.Sistema,
                idAreaAcademica = modelo.IdAreaAcademica,
                idEntidadAcademica = modelo.IdEntidadAcademica,
                idProgramaEducativo = modelo.IdProgramaEducativo
            });

            return Json(new { Url = url });
        }

        [HttpPost]
        public async Task<IActionResult> GuardarPlanEstudiosAsync(GuardarNuevoPlanViewModel modelo)
        {
            var modeloAnterior = new CargarPlanPaso2ViewModel
            {
                Region = modelo.Region,
                IdProgramaEducativo = modelo.IdProgramaEducativo,
                NombrePrograma = modelo.NombrePrograma,
                NombreArea = modelo.NombreArea,
                Plan = modelo.Plan,
                Sistema = modelo.Sistema,
                Recarga = true
            };

            try
            {
                var experienciasJson = HttpContext.Session.GetString("Experiencias");
                if (!string.IsNullOrEmpty(experienciasJson))
                {
                    var experiencias = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(experienciasJson);
                    if (experiencias?.Count == 0)
                    {
                        _logger.LogError("{NOMBRE_LOGGER} La lista de Experiencias Educativas está vacía.", NOMBRE_LOGGER);
                        TempData["Error"] = "La lista de Experiencias Educativas no puede estar vacía.";
                        return await CargarPlanPaso2(modeloAnterior);
                    }

                    var rutaArchivo = HttpContext.Session.GetString("Ruta");
                    if (!string.IsNullOrEmpty(rutaArchivo))
                    {
                        var nombreArchivo = HttpContext.Session.GetString("NombreArchivo");
                        if (!string.IsNullOrEmpty(nombreArchivo))
                        {
                            var archivoDTO = new ArchivoPlanEstudiosDTO
                            {
                                Ruta = rutaArchivo,
                                NombreArchivo = nombreArchivo
                            };

                            await _planEstudiosService.AgregarAsync(new CrearPlanEstudiosDTO
                            {
                                IdProgramaEducativo = (int)modelo.IdProgramaEducativo,
                                Nombre = modelo.Plan,
                                Sistema = modelo.Sistema,
                                Archivo = archivoDTO,
                                ExperienciasEducativas = experiencias.Select(ee => new AgregarExperienciaEducativaDTO
                                {
                                    Codigo = ee.Codigo,
                                    Nombre = ee.Nombre,
                                    PerfilDocente = ee.PerfilDocente
                                }).ToList()
                            });

                            System.IO.File.Delete(rutaArchivo);
                            HttpContext.Session.Clear();
                            TempData["Success"] = "Plan de estudios guardado con éxito.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            _logger.LogError("{NOMBRE_LOGGER} El nombre del archivo es nulo.", NOMBRE_LOGGER);
                            TempData["Error"] = "No se pudo guardar el Plan de Estudios, inténtelo de nuevo más tarde.";
                            return RedirectToAction(nameof(Index));
                        }
                    }
                    else
                    {
                        _logger.LogError("{NOMBRE_LOGGER} La ruta del archivo es nula.", NOMBRE_LOGGER);
                        TempData["Error"] = "No se pudo guardar el Plan de Estudios, inténtelo de nuevo más tarde.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    _logger.LogError("{NOMBRE_LOGGER} La lista de Experiencias está vacía.", NOMBRE_LOGGER);
                    TempData["Error"] = "No se pudo guardar el Plan de Estudios, inténtelo de nuevo más tarde.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (ValidacionExcepction vx)
            {
                _logger.LogError(vx, "{NOMBRE_LOGGER} Error al guardar el plan de estudios.", NOMBRE_LOGGER);
                TempData["Error"] = $"Error: {vx.Message}";
                if (vx.Codigo.Contains("422"))
                    TempData["Error"] = "No se pudo guardar el Plan de Estudios.";
                return await CargarPlanPaso2(modeloAnterior);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{NOMBRE_LOGGER} Error inesperado.", NOMBRE_LOGGER);
                TempData["Error"] = "No se pudo guardar el Plan de Estudios, inténtelo de nuevo más tarde.";
                return RedirectToAction(nameof(CargarPlanPaso1), new CargarPlanPaso1ViewModel());
            }
        }

        //Gestionar Experiencias
        [HttpGet]
        public JsonResult AgregarExperienciaEducativaCreacion(string codigo, string nombre, string perfilDocente)
        {
            bool error = false;
            DatosExperienciaEducativaDTO experiencia = new();

            var listaEeJson = HttpContext.Session.GetString("Experiencias");
            if (!string.IsNullOrEmpty(listaEeJson))
            {
                var listaEe = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(listaEeJson);
                experiencia = new DatosExperienciaEducativaDTO
                {
                    Codigo = codigo,
                    Nombre = nombre,
                    PerfilDocente = perfilDocente
                };

                listaEe.Add(experiencia);

                listaEeJson = JsonSerializer.Serialize(listaEe);
                HttpContext.Session.SetString("Experiencias", listaEeJson);
            }
            else
                error = true;

            if (error)
            {
                _logger.LogError("{NOMBRE_LOGGER} Error al agregar la Experiencia Educativa.", NOMBRE_LOGGER);
                TempData["Error"] = "Ha ocurrido un error, inténtelo nuevamente.";
                return Json(new { error = true });
            }

            return Json(new { experiencia, error = false });
        }

        [HttpGet]
        public JsonResult EditarExperienciaEducativaCreacion(string codigo, string nombre, string perfilDocente, string codigoOriginal)
        {
            bool error = false;
            List<AgregarExperienciaEducativaDTO> listaEe = [];

            var listaEeJson = HttpContext.Session.GetString("Experiencias");
            if (!string.IsNullOrEmpty(listaEeJson))
            {
                listaEe = JsonSerializer.Deserialize<List<AgregarExperienciaEducativaDTO>>(listaEeJson);
                var experiencia = listaEe.FirstOrDefault(ee => ee.Codigo == codigoOriginal);

                if (experiencia != null)
                {
                    experiencia.Codigo = codigo;
                    experiencia.Nombre = nombre;
                    experiencia.PerfilDocente = perfilDocente;
                    listaEeJson = JsonSerializer.Serialize(listaEe);
                    HttpContext.Session.SetString("Experiencias", listaEeJson);
                }
                else
                {
                    _logger.LogError("{NOMBRE_LOGGER} Error al obtener la Experiencia Educativa.", NOMBRE_LOGGER);
                    error = true;
                }
            }
            else
            {
                _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias Educativas.", NOMBRE_LOGGER);
                error = true;
            }

            if (error)
            {
                TempData["Error"] = "Ha ocurrido un error, inténtelo nuevamente.";
                return Json(new { error = true });
            }

            return Json(new { experiencias = listaEe, error = false });
        }



        [HttpGet]
        public JsonResult EliminarExperienciaEducativaCreacion(string codigo)
        {
            List<DatosExperienciaEducativaDTO> listaEe = [];
            bool error = false;

            var listaEeJson = HttpContext.Session.GetString("Experiencias");
            if (!string.IsNullOrEmpty(listaEeJson))
            {
                listaEe = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(listaEeJson);
                var experiencia = listaEe.FirstOrDefault(ee => ee.Codigo == codigo);
                if (experiencia != null)
                {
                    listaEe.Remove(experiencia);
                    listaEeJson = JsonSerializer.Serialize(listaEe);
                    HttpContext.Session.SetString("Experiencias", listaEeJson);
                }
                else
                {
                    _logger.LogError("{NOMBRE_LOGGER} Error al obtener la Experiencia Educativa.", NOMBRE_LOGGER);
                    error = true;
                }
            }
            else
            {
                _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias Educativas.", NOMBRE_LOGGER);
                error = true;
            }

            if (error)
            {
                TempData["Error"] = "Ha ocurrido un error, inténtelo nuevamente.";
                return Json(new { error = true });
            }

            return Json(new { experiencias = listaEe, error = false });
        }

        // ==========
        // EDITAR
        // ==========

        //Vista
        [HttpGet]
        public async Task<IActionResult> EditarPlanAsync(int idPlanEstudios, bool recarga)
        {
            var vista = new EditarPlanViewModel();
            var error = false;
            if (recarga)
            {
                var resultado = await recargarEdicionPlanAsync(idPlanEstudios);
                vista = resultado.modelo;
                error = resultado.error;
            }
            else
            {
                var resultado = await inicializarEdicionPlanAsync(idPlanEstudios);
                vista = resultado.modelo;
                error = resultado.error;
            }

            if (error)
            {
                return RedirectToAction("Index");
            }

            return View("EditarPlan", vista);
        }

        private async Task<(EditarPlanViewModel? modelo, bool error)> inicializarEdicionPlanAsync(int idPlanEstudios)
        {
            try
            {
                var planEstudios = await _planEstudiosService.ObtenerPorIdAsync(idPlanEstudios);
                var vista = new EditarPlanViewModel
                {
                    IdPlanEstudios = planEstudios.IdPlanEstudios,
                    Programa = planEstudios.NombreProgramaEducativo,
                    Area = planEstudios.NombreAreaAcademica,
                    Plan = planEstudios.Nombre,
                    Sistema = planEstudios.Modalidad,
                };

                var eeNuevas = new List<AgregarExperienciaEducativaDTO>();
                HttpContext.Session.SetString("EeNuevas", JsonSerializer.Serialize(eeNuevas));
                var eeEditadas = new List<DatosExperienciaEducativaDTO>();
                HttpContext.Session.SetString("EeEditadas", JsonSerializer.Serialize(eeEditadas));
                var eeEliminadas = new List<int>();
                HttpContext.Session.SetString("EeEliminadas", JsonSerializer.Serialize(eeEliminadas));
                var listaEe = planEstudios.ExperienciasEducativas;
                HttpContext.Session.SetString("Experiencias", JsonSerializer.Serialize(listaEe));

                var tabla = LlenarTablaGestionExperiencias(listaEe);
                tabla.TableId = "tablaExperiencias";
                tabla.Pagination = new PaginationInfo
                {
                    PageSize = 10,
                    TotalItems = listaEe.Count(),
                    PaginationMode = "client"
                };

                vista.Table = tabla;

                return (vista, false);
            }
            catch (ValidacionExcepction vx)
            {
                _logger.LogError(vx, "{NOMBRE_LOGGER} [EditarPlan] Error al cargar el Plan de Estudios.", NOMBRE_LOGGER);
                TempData["Error"] = $"Error: {vx.Message}";
                return (null, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{NOMBRE_LOGGER} [EditarPlan] Error inesperado.", NOMBRE_LOGGER);
                TempData["Error"] = "Ha ocurrido un error, inténtalo de nuevo más tarde.";
                return (null, true);
            }

        }

        private async Task<(EditarPlanViewModel? modelo, bool error)> recargarEdicionPlanAsync(int idPlanEstudios)
        {
            try
            {
                var planEstudios = await _planEstudiosService.ObtenerPorIdAsync(idPlanEstudios);
                var vista = new EditarPlanViewModel
                {
                    IdPlanEstudios = planEstudios.IdPlanEstudios,
                    Programa = planEstudios.NombreProgramaEducativo,
                    Area = planEstudios.NombreAreaAcademica,
                    Plan = planEstudios.Nombre,
                    Sistema = planEstudios.Modalidad,
                };

                var listaEeJson = HttpContext.Session.GetString("Experiencias");
                if (!string.IsNullOrEmpty(listaEeJson))
                {
                    var listaEe = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(listaEeJson);
                    var tabla = LlenarTablaGestionExperiencias(listaEe);
                    tabla.TableId = "tablaExperiencias";
                    tabla.Pagination = new PaginationInfo
                    {
                        PageSize = 10,
                        TotalItems = listaEe.Count(),
                        PaginationMode = "client"
                    };

                    vista.Table = tabla;

                    return (vista, false);
                }
                else
                {
                    _logger.LogError("{NOMBRE_LOGGER} [EditarPlan] La lista de Experiencias está vacía.", NOMBRE_LOGGER);
                    TempData["Error"] = "Error al editar el Plan de Estudios, inténtelo de nuevo más tarde.";
                    return (null, true);
                }
            }
            catch (ValidacionExcepction vx)
            {
                _logger.LogError(vx, "{NOMBRE_LOGGER} [EditarPlan] Error al cargar el Plan de Estudios.", NOMBRE_LOGGER);
                TempData["Error"] = $"Error: {vx.Message}";
                return (null, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{NOMBRE_LOGGER} [EditarPlan] Error inesperado.", NOMBRE_LOGGER);
                TempData["Error"] = "Ha ocurrido un error, inténtalo de nuevo más tarde.";
                return (null, true);
            }

        }

        [HttpPost]
        public async Task<JsonResult> CargarNuevoArchivo(IFormFile archivo)
        {
            try
            {
                await guardarArchivoTemporalmente(archivo);
                List<DatosExperienciaEducativaDTO> listaEeNueva = [];
                bool error = false;

                var ruta = HttpContext.Session.GetString("Ruta");
                if (!string.IsNullOrEmpty(ruta))
                {
                    var nombreArchivo = HttpContext.Session.GetString("NombreArchivo");
                    if (!string.IsNullOrEmpty(nombreArchivo))
                    {
                        var archivoDTO = new ArchivoPlanEstudiosDTO
                        {
                            Ruta = ruta,
                            NombreArchivo = nombreArchivo
                        };
                        listaEeNueva = await ProcesarArchivoAsync(archivoDTO);

                        var eeNuevas = new List<AgregarExperienciaEducativaDTO>();
                        foreach (var ee in listaEeNueva)
                        {
                            eeNuevas.Add(new AgregarExperienciaEducativaDTO
                            {
                                Codigo = ee.Codigo,
                                Nombre = ee.Nombre,
                                PerfilDocente = ee.PerfilDocente
                            });
                        }
                        HttpContext.Session.SetString("EeNuevas", JsonSerializer.Serialize(eeNuevas));

                        var listaEeJson = HttpContext.Session.GetString("Experiencias");
                        if (!string.IsNullOrEmpty(listaEeJson))
                        {
                            var listaEeVieja = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(listaEeJson);
                            var eeEliminadasJson = HttpContext.Session.GetString("EeEliminadas");
                            if (!string.IsNullOrEmpty(eeEliminadasJson))
                            {
                                var eeEliminadas = JsonSerializer.Deserialize<List<int>>(eeEliminadasJson);
                                foreach (var ee in listaEeVieja)
                                {
                                    if (ee.IdExperienciaEducativa > 0 && !eeEliminadas.Contains(ee.IdExperienciaEducativa))
                                        eeEliminadas.Add(ee.IdExperienciaEducativa);
                                }
                                HttpContext.Session.SetString("EeEliminadas", JsonSerializer.Serialize(eeEliminadas));
                                HttpContext.Session.SetString("EeEditadas", JsonSerializer.Serialize(new List<DatosExperienciaEducativaDTO>()));
                                HttpContext.Session.SetString("Experiencias", JsonSerializer.Serialize(listaEeNueva));
                            }
                            else
                            {
                                _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias eliminadas.", NOMBRE_LOGGER);
                                error = true;
                            }
                        }
                        else
                        {
                            _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias Educativas.", NOMBRE_LOGGER);
                            error = true;
                        }
                    }
                    else
                    {
                        _logger.LogError("{NOMBRE_LOGGER} El nombre del archivo es nulo.", NOMBRE_LOGGER);
                        error = true;
                    }
                }
                else
                {
                    _logger.LogError("{NOMBRE_LOGGER} La ruta es nula.", NOMBRE_LOGGER);
                    error = true;
                }

                if (error)
                {
                    TempData["Error"] = "No se pudo procesar el archivo.";
                    return Json(new { error = true });
                }

                return Json( new { experiencias = listaEeNueva, error = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{NOMBRE_LOGGER} Error inesperado.", NOMBRE_LOGGER);
                TempData["Error"] = "Ha ocurrido un error, inténtelo de nuevo más tarde.";
                return Json(new { error = true });
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> GuardarPlanEstudiosEditadoAsync(EditarPlanViewModel modelo)
        {
            try
            {
                var eeNuevas = HttpContext.Session.GetString("EeNuevas");
                if (string.IsNullOrEmpty(eeNuevas))
                {
                    _logger.LogError("{NOMBRE_LOGGER} La lista de Experiencias nuevas es nula.", NOMBRE_LOGGER);
                    TempData["Error"] = "No se pudo editar el Plan de Estudios, inténtelo nuevamente.";
                    return RedirectToAction(nameof(Index));
                }
                var eeEditadas = HttpContext.Session.GetString("EeEditadas");
                if (string.IsNullOrEmpty(eeEditadas))
                {
                    _logger.LogError("{NOMBRE_LOGGER} La lista de Experiencias editadas es nula.", NOMBRE_LOGGER);
                    TempData["Error"] = "No se pudo editar el Plan de Estudios, inténtelo nuevamente.";
                    return RedirectToAction(nameof(Index));
                }
                var eeEliminadas = HttpContext.Session.GetString("EeEliminadas");
                if (string.IsNullOrEmpty(eeEliminadas))
                {
                    _logger.LogError("{NOMBRE_LOGGER} La lista de Experiencias eliminadas es nula.", NOMBRE_LOGGER);
                    TempData["Error"] = "No se pudo editar el Plan de Estudios, inténtelo nuevamente.";
                    return RedirectToAction(nameof(Index));
                }
                var listaEeJson = HttpContext.Session.GetString("Experiencias");
                if (!string.IsNullOrEmpty(listaEeJson))
                {
                    var listaEe = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(listaEeJson);
                    if (listaEe.Count <= 0)
                    {
                        _logger.LogError("{NOMBRE_LOGGER} [GuardarPlanEditado].", NOMBRE_LOGGER);
                        TempData["Error"] = "La lista de Experiencias Educativas no puede estar vacía.";
                        return await EditarPlanAsync(modelo.IdPlanEstudios, true);
                    }
                }

                var plan = new EditarPlanEstudiosDTO
                {
                    IdPlanEstudios = modelo.IdPlanEstudios,
                    NuevaLista = modelo.NuevoArchivo,
                    ExperienciasNuevas = JsonSerializer.Deserialize<List<AgregarExperienciaEducativaDTO>>(eeNuevas),
                    ExperienciasEditadas = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(eeEditadas),
                    IdsExperienciasEliminadas = JsonSerializer.Deserialize<List<int>>(eeEliminadas)
                };

                var rutaArchivo = HttpContext.Session.GetString("Ruta");
                if (modelo.NuevoArchivo)
                {
                    if (string.IsNullOrEmpty(rutaArchivo))
                    {
                        _logger.LogError("{NOMBRE_LOGGER} La ruta del archivo es nula.", NOMBRE_LOGGER);
                        TempData["Error"] = "No se pudo editar el Plan de Estudios, inténtelo nuevamente.";
                        return RedirectToAction(nameof(Index));
                    }
                    var nombreArchivo = HttpContext.Session.GetString("NombreArchivo");
                    if (string.IsNullOrEmpty(nombreArchivo))
                    {
                        _logger.LogError("{NOMBRE_LOGGER} El nombre del archivo es nulo.", NOMBRE_LOGGER);
                        TempData["Error"] = "No se pudo editar el Plan de Estudios, inténtelo nuevamente.";
                        return RedirectToAction(nameof(Index));
                    }

                    plan.Archivo = new ArchivoPlanEstudiosDTO
                    {
                        Ruta = rutaArchivo,
                        NombreArchivo = nombreArchivo
                    };

                    await _planEstudiosService.EditarAsync(plan);
                    System.IO.File.Delete(rutaArchivo);
                    TempData["Success"] = "Plan de estudios guardado con éxito.";
                    return RedirectToAction(nameof(Index));
                }

                await _planEstudiosService.EditarAsync(plan);
                TempData["Success"] = "Plan de estudios guardado con éxito.";
                return RedirectToAction(nameof(Index));

            }
            catch (ValidacionExcepction vx)
            {
                _logger.LogError(vx, "{NOMBRE_LOGGER} [GuardarPlanEditado].", NOMBRE_LOGGER);
                TempData["Error"] = $"Error: {vx.Message}";
                if (vx.Codigo.Contains("422"))
                    TempData["Error"] = "No se pudo editar el Plan de Estudios, inténtelo nuevamente.";
                return await EditarPlanAsync(modelo.IdPlanEstudios, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{NOMBRE_LOGGER} [GuardarPlanEditado] Error inesperado.", NOMBRE_LOGGER);
                TempData["Error"] = $"No se pudo editar el Plan de Estudios, inténtelo más tarde.";
                return RedirectToAction(nameof(Index));
            }
        }

        //Gestion experiencias
        [HttpGet]
        public JsonResult AgregarExperienciaEducativaEdicion(string codigo, string nombre, string perfilDocente)
        {
            DatosExperienciaEducativaDTO experiencia = new();
            bool error = false;

            var eeNuevasJson = HttpContext.Session.GetString("EeNuevas");
            if (!string.IsNullOrEmpty(eeNuevasJson))
            {
                var eeNuevas = JsonSerializer.Deserialize<List<AgregarExperienciaEducativaDTO>>(eeNuevasJson);
                var listaEeJson = HttpContext.Session.GetString("Experiencias");
                if (!string.IsNullOrEmpty(listaEeJson))
                {
                    var listaEe = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(listaEeJson);
                    experiencia = new DatosExperienciaEducativaDTO
                    {
                        Codigo = codigo,
                        Nombre = nombre,
                        PerfilDocente = perfilDocente
                    };

                    eeNuevas.Add(new AgregarExperienciaEducativaDTO
                    {
                        Codigo = codigo,
                        Nombre = nombre,
                        PerfilDocente = perfilDocente
                    });
                    listaEe.Add(experiencia);

                    eeNuevasJson = JsonSerializer.Serialize(eeNuevas);
                    HttpContext.Session.SetString("EeNuevas", eeNuevasJson);
                    listaEeJson = JsonSerializer.Serialize(listaEe);
                    HttpContext.Session.SetString("Experiencias", listaEeJson);
                }
                else
                {
                    _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias Educativas.", NOMBRE_LOGGER);
                    error = true;
                }
            }
            else
            {
                _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias Educativas nuevas.", NOMBRE_LOGGER);
                error = true;

            }

            if (error)
            {
                TempData["Error"] = "Ha ocurrido un error, inténtelo nuevamente.";
                return Json(new { error = true });
            }

            return Json(new { experiencia, error = false });
        }

        [HttpGet]
        public JsonResult EditarExperienciaEducativaEdicion(string codigo, string nombre, string perfilDocente, string codigoOriginal)
        {
            List<DatosExperienciaEducativaDTO> listaEe = [];
            bool error = false;

            var eeEditadasJson = HttpContext.Session.GetString("EeEditadas");
            if (!string.IsNullOrEmpty(eeEditadasJson))
            {
                var listaEeJson = HttpContext.Session.GetString("Experiencias");
                if (!string.IsNullOrEmpty(listaEeJson))
                {
                    listaEe = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(listaEeJson);
                    var eeEditadas = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(eeEditadasJson);
                    var experiencia = listaEe.FirstOrDefault(ee => ee.Codigo == codigoOriginal);

                    if (experiencia != null)
                    {
                        experiencia.Codigo = codigo;
                        experiencia.Nombre = nombre;
                        experiencia.PerfilDocente = perfilDocente;

                        if (experiencia.IdExperienciaEducativa > 0)
                        {
                            var experienciaEditada = eeEditadas.FirstOrDefault(ee => ee.Codigo == codigoOriginal);
                            if (experienciaEditada != null)
                                experienciaEditada = experiencia;
                            else
                            {
                                eeEditadas.Add(experiencia);
                            }

                            eeEditadasJson = JsonSerializer.Serialize(eeEditadas);
                            HttpContext.Session.SetString("EeEditadas", eeEditadasJson);
                        }
                        else
                        {
                            var eeNuevasJson = HttpContext.Session.GetString("EeNuevas");
                            if (!string.IsNullOrEmpty(eeNuevasJson))
                            {
                                var eeNuevas = JsonSerializer.Deserialize<List<AgregarExperienciaEducativaDTO>>(eeNuevasJson);
                                var experienciaNueva = eeNuevas.FirstOrDefault(ee => ee.Codigo == codigoOriginal);
                                if (experienciaNueva != null)
                                {
                                    experienciaNueva.Codigo = experiencia.Codigo;
                                    experienciaNueva.Nombre = experiencia.Nombre;
                                    experienciaNueva.PerfilDocente = experiencia.PerfilDocente;
                                }
                                else
                                {
                                    eeNuevas.Add(new AgregarExperienciaEducativaDTO
                                    {
                                        Codigo = experiencia.Codigo,
                                        Nombre = experiencia.Nombre,
                                        PerfilDocente = experiencia.PerfilDocente
                                    });
                                }

                                eeNuevasJson = JsonSerializer.Serialize(eeNuevas);
                                HttpContext.Session.SetString("EeNuevas", eeNuevasJson);
                            }
                            else
                            {
                                _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias nuevas.", NOMBRE_LOGGER);
                                error = true;
                            }
                        }

                        listaEeJson = JsonSerializer.Serialize(listaEe);
                        HttpContext.Session.SetString("Experiencias", listaEeJson);
                    }
                    else
                    {
                        _logger.LogError("{NOMBRE_LOGGER} Error al obtener la Experiencia Educativa.", NOMBRE_LOGGER);
                        error = true;
                    }
                }
                else
                {
                    _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias.", NOMBRE_LOGGER);
                    error = true;
                }
            }
            else
            {
                _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias Editadas.", NOMBRE_LOGGER);
                error = true;
            }

            if (error)
            {
                TempData["Error"] = "Ha ocurrido un error, inténtelo nuevamente.";
                return Json(new { error = true });
            }

            return Json(new { experiencias = listaEe, error = false });
        }

        [HttpGet]
        public JsonResult EliminarExperienciaEducativaEdicion(string codigo)
        {
            List<DatosExperienciaEducativaDTO> listaEe = [];
            bool error = false;

            var eeEliminadasJson = HttpContext.Session.GetString("EeEliminadas");
            if (!string.IsNullOrEmpty(eeEliminadasJson))
            {
                var listaEeJson = HttpContext.Session.GetString("Experiencias");
                if (!string.IsNullOrEmpty(listaEeJson))
                {
                    listaEe = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(listaEeJson);
                    var eeEliminadas = JsonSerializer.Deserialize<List<int>>(eeEliminadasJson);

                    var experiencia = listaEe.FirstOrDefault(ee => ee.Codigo == codigo);
                    if (experiencia != null)
                    {
                        if (experiencia.IdExperienciaEducativa > 0)
                        {
                            listaEe.Remove(experiencia);
                            eeEliminadas.Add(experiencia.IdExperienciaEducativa);

                            eeEliminadasJson = JsonSerializer.Serialize(eeEliminadas);
                            HttpContext.Session.SetString("EeEliminadas", eeEliminadasJson);
                        }
                        else
                        {
                            var eeNuevasJson = HttpContext.Session.GetString("EeNuevas");
                            if (!string.IsNullOrEmpty(eeNuevasJson))
                            {
                                var eeNuevas = JsonSerializer.Deserialize<List<AgregarExperienciaEducativaDTO>>(eeNuevasJson);
                                var experienciaNueva = eeNuevas.FirstOrDefault(ee => ee.Codigo == codigo);
                                if (experienciaNueva != null)
                                {
                                    eeNuevas.Remove(experienciaNueva);
                                    listaEe.Remove(experiencia);

                                    eeNuevasJson = JsonSerializer.Serialize(eeNuevas);
                                    HttpContext.Session.SetString("EeNuevas", eeNuevasJson);
                                }
                                else
                                {
                                    _logger.LogError("{NOMBRE_LOGGER} Error al obtener la Experiencia Educativa nueva.", NOMBRE_LOGGER);
                                    error = true;
                                }
                            }
                            else
                            {
                                _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias nuevas.", NOMBRE_LOGGER);
                                error = true;
                            }
                        }

                        listaEeJson = JsonSerializer.Serialize(listaEe);
                        HttpContext.Session.SetString("Experiencias", listaEeJson);
                    }
                    else
                    {
                        _logger.LogError("{NOMBRE_LOGGER} Error al obtener la Experiencia Educativa.", NOMBRE_LOGGER);
                        error = true;
                    }
                }
                else
                {
                    _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias.", NOMBRE_LOGGER);
                    error = true;
                }
            }
            else
            {
                _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias eliminadas.", NOMBRE_LOGGER);
                error = true;
            }

            if (error)
            {
                TempData["Error"] = "Ha ocurrido un error, inténtelo nuevamente.";
                return Json(new { error = true });
            }

            return Json(new { experiencias = listaEe, error = false });
        }

        /// ==========
        // Utils
        // ==========
        [HttpGet]
        public async Task<JsonResult> ObtenerAreasAsync(int? idAreaAcademica)
        {
            var areas = await generarCatalogoAreasAsync(idAreaAcademica);

            return Json(areas);
        }

        [HttpGet]
        public async Task<JsonResult> ObtenerEntidadesAsync(int idAreaAcademica, string region, int? idEntidadAcademica)
        {
            var entidades = await generarCatalogoEntidadesAsync(idAreaAcademica, region, idEntidadAcademica);

            return Json(entidades);
        }

        [HttpGet]
        public async Task<JsonResult> ObtenerProgramasAsync(int idAreaAcademica, string region, int idEntidadAcademica, int? idProgramaEducativo)
        {
            var entidades = await generarCatalogoProgramasAsync(idAreaAcademica, region, idEntidadAcademica, idProgramaEducativo);

            return Json(entidades);
        }

        private List<OptionModel> generarCatalogoRegiones()
        {
            return Constantes.Regiones
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r
                })
                .ToList();
        }

        [HttpGet]
        public async Task<IActionResult> CancelarAccionAsync()
        {
            var rutaArchivo = HttpContext.Session.GetString("Ruta");
            if (!string.IsNullOrEmpty(rutaArchivo))
                System.IO.File.Delete(rutaArchivo);

            var url = Url.Action(nameof(Index));

            return Json(new { Url = url });
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

            var entidades = await _entidadAcademicaService.ObtenerCatalogoAsync(filtros);

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

        private List<OptionModel> generarCatalogoModalidades()
        {
            return Constantes.Modalidades
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r
                })
                .ToList();
        }

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

        private TableModel LlenarTablaGestionExperiencias(List<DatosExperienciaEducativaDTO> experiencias)
        {
            var tabla = new TableModel();

            if (experiencias.Count == 0)
            {
                tabla.Headers = new List<string> { "Codigo", "Experiencia Educativa", "Perfil Docente", "Acciones" };
                tabla.Rows = new TableRowModel[]
                    {
                        new TableRowModel
                        {
                            Cells = []
                        }
                    }.ToList();
                return tabla;
            }
            else
            {
                tabla = tabla = new TableModel
                {
                    Headers = new List<string> { "Codigo", "Experiencia Educativa", "Perfil Docente", "Acciones" },
                    Rows = experiencias.Select(ee => new TableRowModel
                    {
                        RowId = ee.Codigo,
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
                            new TableCellModel()
                            {
                                Actions = new List<TableActionModel>
                                {
                                    new()
                                    {
                                        Accion = "editar",
                                        OnClick = $"abrirModalEditarExperiencia('{ee.Codigo}', '{ee.Nombre}', '{ee.PerfilDocente}')"
                                    },
                                    new()
                                    {
                                        Accion = "eliminar",
                                        OnClick = $"abrirModalEliminarExperiencia('{ee.Codigo}')"
                                    }
                                }
                            }
                        }
                    }).ToList()
                };
            }

            return tabla;
        }

        private async Task guardarArchivoTemporalmente(IFormFile archivo)
        {
            var carpetaTemp = Path.Combine(_environment.ContentRootPath, "TempUploads");
            Directory.CreateDirectory(carpetaTemp);
            var extension = Path.GetExtension(archivo.FileName);
            var rutaArchivo = Path.Combine(carpetaTemp, $"{Guid.NewGuid()}{extension}");
            using var stream = new FileStream(rutaArchivo, FileMode.Create);
            await archivo.CopyToAsync(stream);

            HttpContext.Session.SetString("Ruta", rutaArchivo);
            HttpContext.Session.SetString("NombreArchivo", archivo.FileName);
        }

        private TableModel generarTablaError()
        {
            return new TableModel
            {
                Headers = new List<string>
                    {
                        ""
                    },
                Rows = new TableRowModel[]
                    {
                        new TableRowModel
                        {
                            Cells = new List<TableCellModel>
                            {
                                new() { Value = "Ha ocurrido un error, inténtalo de nuevo más tarde." }
                            }
                        }
                    }.ToList(),
            };
        }

        private async Task<CargarPlanPaso1ViewModel> recargarCombosAsync(CargarPlanPaso1ViewModel modelo)
        {
            modelo.ListaRegiones = generarCatalogoRegiones();
            modelo.ListaSistema = generarCatalogoModalidades();
            modelo.ListaAreas = await generarCatalogoAreasAsync(modelo.IdAreaAcademica);
            if (modelo.IdAreaAcademica.HasValue && modelo.IdAreaAcademica != 0)
            {
                modelo.ListaEntidades = await generarCatalogoEntidadesAsync((int)modelo.IdAreaAcademica, modelo.Region, modelo.IdEntidadAcademica);
                if (modelo.IdEntidadAcademica.HasValue && modelo.IdEntidadAcademica != 0)
                    modelo.ListaProgramas = await generarCatalogoProgramasAsync((int)modelo.IdAreaAcademica, modelo.Region, (int)modelo.IdEntidadAcademica, modelo.IdProgramaEducativo);
            }

            return modelo;
        }
    }
}
