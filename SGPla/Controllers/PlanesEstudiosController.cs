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
using System.Text.Json;

namespace SGPla.Controllers
{
    public class PlanesEstudiosController : Controller
    {
        private readonly IPlanEstudiosService _planEstudiosService;
        private readonly IAreaAcademicaService _areaAcademicaService;
        private readonly IEntidadAcademicaService _entidadAcademicaService;
        private readonly IProgramaEducativoService _programaEducativoService;
        private readonly ILogger<PlanesEstudiosController> _logger;
        private readonly IWebHostEnvironment _environment;
        private const string NOMBRE_LOGGER = "FRONT-PLANES:";
        private int paginaActual = 1;

        public PlanesEstudiosController(
            IPlanEstudiosService planEstudiosService,
            IAreaAcademicaService areaAcademicaService,
            IEntidadAcademicaService entidadAcademicaService,
            IProgramaEducativoService programaEducativoService,
            ILogger<PlanesEstudiosController> logger,
            IWebHostEnvironment environment)
        {
            _planEstudiosService = planEstudiosService;
            _areaAcademicaService = areaAcademicaService;
            _entidadAcademicaService = entidadAcademicaService;
            _programaEducativoService = programaEducativoService;
            _logger = logger;
            _environment = environment;
        }

        /*
         * VISTAS
         */

        //Ver todos los Planes de Estudios
        [HttpGet]
        public async Task<IActionResult> Index(string? busqueda, string? region, int? idAreaAcademica, int? idEntidadAcademica, int? idProgramaEducativo, int pagina = 1, int cantidad = 10)
        {
            paginaActual = pagina;

            var regionesCombo = generarCatalogoRegiones();
            var areasCombo = new List<OptionModel>();
            var entidadesCombo = new List<OptionModel>();
            var programasCombo = new List<OptionModel>();

            //Llenar Areas
            if (!region.IsNullOrEmpty())
                areasCombo = await generarCatalogoAreasAsync(idAreaAcademica);
            else
                idAreaAcademica = null;

            //Llenar Entidades
            if (idAreaAcademica.HasValue)
                entidadesCombo = await generarCatalogoEntidadesAsync(idAreaAcademica.Value, region, idEntidadAcademica);
            else
                idEntidadAcademica = null;

            //Llenar Programas Educativos
            if (idEntidadAcademica.HasValue)
                programasCombo = await generarCatalogoProgramasAsync(idAreaAcademica.Value, region, idEntidadAcademica.Value, idProgramaEducativo);
            else
                idProgramaEducativo = null;

            return View(new IndexViewModel
            {
                Table = await LlenarTablaIndexAsync(busqueda, region, idAreaAcademica, idEntidadAcademica, idProgramaEducativo, pagina, cantidad),
                Regiones = regionesCombo,
                Areas = areasCombo,
                Entidades = entidadesCombo,
                ProgramasEducativos = programasCombo,
                PaginaActual = paginaActual,
                CantidadPorPaginas = cantidad
            });
        }

        //Ver Plan de Estudios
        [HttpGet]
        public async Task<IActionResult> VerPlanEstudiosAsync(int idPlanEstudios, int pagina = 1, int cantidad = 10)
        {
            _logger.LogInformation("{NOMBRE_LOGGER} Visualizando Plan de Estudios con Id: {id}", NOMBRE_LOGGER, idPlanEstudios);
            paginaActual = pagina;
            if (idPlanEstudios == 0) return BadRequest();

            try
            {
                var plan = await _planEstudiosService.ObtenerPorIdAsync(idPlanEstudios);
                if (plan == null)
                    return NotFound();

                HttpContext.Session.Clear();

                return View(new VerPlanEstudiosViewModel
                {
                    IdPlanEstudios = plan.IdPlanEstudios,
                    NombreProgramaEducativo = plan.NombreProgramaEducativo,
                    Modalidad = plan.Modalidad,
                    Nombre = plan.Nombre,
                    NombreAreaAcademica = plan.NombreAreaAcademica,
                    ExperienciasEducativas = plan.ExperienciasEducativas,
                    Table = LlenarTablaVerPlanEstudios(plan.ExperienciasEducativas, paginaActual, cantidad),
                    PaginaActual = paginaActual,
                    CantidadPorPaginas = cantidad
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{NOMBRE_LOGGER} Error al obtener detalles del Plan de Estudios con Id: {id}", NOMBRE_LOGGER, idPlanEstudios);
                TempData["Error"] = "Error al cargar los detalles del Plan de Estudios";
                return RedirectToAction(nameof(Index));
            }
        }

        //Cargar Plan Paso 1
        [HttpGet]
        public async Task<IActionResult> CargarPlanPaso1(CargarPlanPaso1ViewModel modelo)
        {
            var region = modelo.Region;
            var plan = modelo.Plan;
            var modalidad = modelo.Sistema;
            var idAreaAcademica = modelo.Area;
            var idEntidadAcademica = modelo.Entidad;
            var idProgramaEducativo = modelo.Programa;

            var regionesCombo = generarCatalogoRegiones();
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
            if (idAreaAcademica.HasValue)
                entidadesCombo = await generarCatalogoEntidadesAsync(idAreaAcademica.Value, region, idEntidadAcademica);
            else
                idEntidadAcademica = null;

            //Llenar Programas Educativos
            if (idEntidadAcademica.HasValue)
                programasCombo = await generarCatalogoProgramasAsync(idAreaAcademica.Value, region, idEntidadAcademica.Value, idProgramaEducativo);
            else
                idProgramaEducativo = null;

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

        //Cargar Plan Paso 2
        [HttpPost]
        public async Task<IActionResult> CargarPlanPaso2(CargarPlanPaso2ViewModel modelo)
        {
            _logger.LogInformation("{NOMBRE_LOGGER} Paso 2 de Cargar Plan de Estudios.", NOMBRE_LOGGER);

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

                var area = await _areaAcademicaService.ObtenerPorIdAsync(modelo.Area);
                var programa = await _programaEducativoService.ObtenerPorIdAsync(modelo.Programa);

                var vista = new CargarPlanPaso2ViewModel
                {
                    Region = modelo.Region,
                    Area = modelo.Area,
                    NombreArea = area.Nombre,
                    Entidad = modelo.Entidad,
                    NombrePrograma = programa.Nombre,
                    Programa = modelo.Programa,
                    Plan = modelo.Plan,
                    Sistema = modelo.Sistema,
                    Table = LlenarTablaGestionExperiencias(listaEe, false)
                };

                return View(vista);
            }
            else
            {
                _logger.LogError("{NOMBRE_LOGGER} La ruta del archivo es nula.", NOMBRE_LOGGER);
                TempData["Error"] = "La ruta del archivo es nula.";
                return RedirectToAction(nameof(CargarPlanPaso1));
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditarPlanAsync(int idPlanEstudios)
        {
            var planEstudios = await _planEstudiosService.ObtenerPorIdAsync(idPlanEstudios);

            var eeNuevas = new List<AgregarExperienciaEducativaDTO>();
            HttpContext.Session.SetString("EeNuevas", JsonSerializer.Serialize(eeNuevas));
            var eeEditadas = new List<DatosExperienciaEducativaDTO>();
            HttpContext.Session.SetString("EeEditadas", JsonSerializer.Serialize(eeEditadas));
            var eeEliminadas = new List<int>();
            HttpContext.Session.SetString("EeEliminadas", JsonSerializer.Serialize(eeEliminadas));
            var listaEe = planEstudios.ExperienciasEducativas;
            HttpContext.Session.SetString("Experiencias", JsonSerializer.Serialize(listaEe));

            return View(new EditarPlanViewModel
            {
                IdPlanEstudios = planEstudios.IdPlanEstudios,
                Programa = planEstudios.NombreProgramaEducativo,
                Area = planEstudios.NombreAreaAcademica,
                Plan = planEstudios.Nombre,
                Sistema = planEstudios.Modalidad,
                Table = LlenarTablaGestionExperiencias(listaEe, true)
            });
        }

        /*
         * Llamadas Http
         */

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

        [HttpPost]
        public async Task<IActionResult> RegresarPaso1Async([FromBody] CargarPlanPaso1ViewModel modelo)
        {
            var rutaArchivo = HttpContext.Session.GetString("Ruta");
            if (!string.IsNullOrEmpty(rutaArchivo))
            {
                System.IO.File.Delete(rutaArchivo);
                HttpContext.Session.Remove("Ruta");
            }

            var url = Url.Action(nameof(CargarPlanPaso1), new CargarPlanPaso1ViewModel
            {
                Region = modelo.Region,
                Area = modelo.Area,
                Entidad = modelo.Entidad,
                Programa = modelo.Programa,
                Plan = modelo.Plan,
                Sistema = modelo.Sistema
            });

            return Json(new { Url = url });
        }

        [HttpPost]
        public async Task<IActionResult> GuardarPlanEstudiosAsync(GuardarNuevoPlanViewModel modelo)
        {
            try
            {
                var jsonEe = HttpContext.Session.GetString("Experiencias");
                if (!string.IsNullOrEmpty(jsonEe))
                {
                    var experiencias = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(jsonEe);
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
                                IdProgramaEducativo = (int)modelo.Programa,
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
                            TempData["Error"] = "El nombre del archivo es nulo.";
                            return RedirectToAction(nameof(CargarPlanPaso1), new CargarPlanPaso1ViewModel());
                        }
                    }
                    else
                    {
                        _logger.LogError("{NOMBRE_LOGGER} La ruta del archivo es nula.", NOMBRE_LOGGER);
                        TempData["Error"] = "La ruta del archivo es nula.";
                        return RedirectToAction(nameof(CargarPlanPaso1), new CargarPlanPaso1ViewModel());
                    }
                }
                else
                {
                    _logger.LogError("{NOMBRE_LOGGER} La lista de Experiencias está vacía.", NOMBRE_LOGGER);
                    TempData["Error"] = "La lista de Experiencias está vacía.";
                    return RedirectToAction(nameof(CargarPlanPaso1), new CargarPlanPaso1ViewModel());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar el Plan de Estudios.");
                TempData["Error"] = "Error al guardar el Plan de Estudios.";
                return RedirectToAction(nameof(CargarPlanPaso1), new CargarPlanPaso1ViewModel());
            }
        }

        [HttpGet]
        public JsonResult AgregarExperienciaEducativaCreacion(string codigo, string nombre, string perfilDocente)
        {
            var listaEeJson = HttpContext.Session.GetString("Experiencias");
            if (!string.IsNullOrEmpty(listaEeJson))
            {
                var listaEe = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(listaEeJson);
                var experiencia = new DatosExperienciaEducativaDTO
                {
                    Codigo = codigo,
                    Nombre = nombre,
                    PerfilDocente = perfilDocente
                };

                listaEe.Add(experiencia);

                listaEeJson = JsonSerializer.Serialize(listaEe);
                HttpContext.Session.SetString("Experiencias", listaEeJson);
                TempData["Success"] = "Experiencia Educativa guardada con éxito.";

                return Json(experiencia);
            }
            else
            {
                _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias Educativas.", NOMBRE_LOGGER);
                TempData["Error"] = "Error al obtener la lista de Experiencias Educativas.";
                return null;
            }
        }

        [HttpGet]
        public JsonResult AgregarExperienciaEducativaEdicion(string codigo, string nombre, string perfilDocente)
        {
            var eeNuevasJson = HttpContext.Session.GetString("EeNuevas");
            if (!string.IsNullOrEmpty(eeNuevasJson))
            {
                var eeNuevas = JsonSerializer.Deserialize<List<AgregarExperienciaEducativaDTO>>(eeNuevasJson);
                var listaEeJson = HttpContext.Session.GetString("Experiencias");
                if (!string.IsNullOrEmpty(listaEeJson))
                {
                    var listaEe = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(listaEeJson);
                    var experiencia = new DatosExperienciaEducativaDTO
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

                    TempData["Success"] = "Experiencia Educativa guardada con éxito.";

                    return Json(experiencia);
                }
                else
                {
                    _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias Educativas.", NOMBRE_LOGGER);
                    TempData["Error"] = "Error al obtener la lista de Experiencias Educativas.";
                    return Json(null);
                }
            }
            else
            {
                _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias Educativas nuevas.", NOMBRE_LOGGER);
                TempData["Error"] = "Error al obtener la lista de Experiencias Educativas nuevas.";
                return Json(null);
            }
        }

        [HttpGet]
        public JsonResult EditarExperienciaEducativaCreacion(string codigo, string nombre, string perfilDocente, string codigoOriginal)
        {
            var listaEeJson = HttpContext.Session.GetString("Experiencias");
            if (!string.IsNullOrEmpty(listaEeJson))
            {
                var listaEe = JsonSerializer.Deserialize<List<AgregarExperienciaEducativaDTO>>(listaEeJson);
                var experiencia = listaEe.FirstOrDefault(ee => ee.Codigo == codigoOriginal);
                
                if (experiencia != null)
                {
                    experiencia.Codigo = codigo;
                    experiencia.Nombre = nombre;
                    experiencia.PerfilDocente = perfilDocente;
                    listaEeJson = JsonSerializer.Serialize(listaEe);
                    HttpContext.Session.SetString("Experiencias", listaEeJson);

                    TempData["Success"] = "Cambios guardados con éxito.";

                    return Json(listaEe);
                }
                else
                {
                    _logger.LogError("{NOMBRE_LOGGER} Error al obtener la Experiencia Educativa.", NOMBRE_LOGGER);
                    TempData["Error"] = "Error al obtener la Experiencia Educativa.";
                    return null;
                }
            }
            else
            {
                _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias Educativas.", NOMBRE_LOGGER);
                TempData["Error"] = "Error al obtener la lista de Experiencias Educativas.";
                return null;
            }
        }

        [HttpGet]
        public JsonResult EditarExperienciaEducativaEdicion(string codigo, string nombre, string perfilDocente, int idExperienciaEducativa)
        {
            var eeEditadasJson = HttpContext.Session.GetString("EeEditadas");
            if (!string.IsNullOrEmpty(eeEditadasJson))
            {
                var listaEeJson = HttpContext.Session.GetString("Experiencias");
                if (!string.IsNullOrEmpty(listaEeJson))
                {
                    var listaEe = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(listaEeJson);
                    var eeEditadas = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(eeEditadasJson);
                    var experiencia = listaEe.FirstOrDefault(ee => ee.IdExperienciaEducativa == idExperienciaEducativa);

                    if (experiencia != null)
                    {
                        experiencia.Codigo = codigo;
                        experiencia.Nombre = nombre;
                        experiencia.PerfilDocente = perfilDocente;
                        
                        eeEditadas.Add(experiencia);
                        eeEditadasJson = JsonSerializer.Serialize(eeEditadas);
                        HttpContext.Session.SetString("EeEditadas", eeEditadasJson);

                        listaEeJson = JsonSerializer.Serialize(listaEe);
                        HttpContext.Session.SetString("Experiencias", listaEeJson);

                        TempData["Success"] = "Cambios guardados con éxito.";

                        return Json(listaEe);
                    }
                    else
                    {
                        _logger.LogError("{NOMBRE_LOGGER} Error al obtener la Experiencia Educativa.", NOMBRE_LOGGER);
                        TempData["Error"] = "Error al obtener la Experiencia Educativa.";
                        return Json(null);
                    }
                }
                else
                {
                    _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias Educativas.", NOMBRE_LOGGER);
                    TempData["Error"] = "Error al obtener la lista de Experiencias Educativas.";
                    return Json(null);
                }
            }
            else
            {
                _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias Educativas.", NOMBRE_LOGGER);
                TempData["Error"] = "Error al obtener la lista de Experiencias Educativas.";
                return Json(null);
            }
        }

        [HttpGet]
        public JsonResult EliminarExperienciaEducativaCreacion(string codigo)
        {
            var listaEeJson = HttpContext.Session.GetString("Experiencias");
            if (!string.IsNullOrEmpty(listaEeJson))
            {
                var listaEe = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(listaEeJson);
                var experiencia = listaEe.FirstOrDefault(ee => ee.Codigo == codigo);
                if (experiencia != null)
                {
                    listaEe.Remove(experiencia);
                    listaEeJson = JsonSerializer.Serialize(listaEe);
                    HttpContext.Session.SetString("Experiencias", listaEeJson);

                    TempData["Success"] = "El elemento ha sido eliminado con éxito.";

                    return Json(listaEe);
                }
                else
                {
                    _logger.LogError("{NOMBRE_LOGGER} Error al obtener la Experiencia Educativa.", NOMBRE_LOGGER);
                    TempData["Error"] = "Error al obtener la Experiencia Educativa.";
                    return Json(null);
                }
            }
            else
            {
                _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias Educativas.", NOMBRE_LOGGER);
                TempData["Error"] = "Error al obtener la lista de Experiencias Educativas.";
                return Json(null);
            }
        }

        [HttpGet]
        public JsonResult EliminarExperienciaEducativaEdicion(int idExperienciaEducativa)
        {
            var eeEliminadasJson = HttpContext.Session.GetString("EeEliminadas");
            if (!string.IsNullOrEmpty(eeEliminadasJson))
            {
                var listaEeJson = HttpContext.Session.GetString("Experiencias");
                if (!string.IsNullOrEmpty(listaEeJson))
                {
                    var listaEe = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(listaEeJson);
                    var eeEliminadas = JsonSerializer.Deserialize<List<int>>(eeEliminadasJson);

                    var experiencia = listaEe.FirstOrDefault(ee => ee.IdExperienciaEducativa == idExperienciaEducativa);
                    if (experiencia != null)
                    {
                        listaEe.Remove(experiencia);
                        eeEliminadas.Add(idExperienciaEducativa);

                        eeEliminadasJson = JsonSerializer.Serialize(eeEliminadas);
                        HttpContext.Session.SetString("EeEliminadas", eeEliminadasJson);

                        listaEeJson = JsonSerializer.Serialize(listaEe);
                        HttpContext.Session.SetString("Experiencias", listaEeJson);

                        TempData["Success"] = "El elemento ha sido eliminado con éxito.";

                        return Json(listaEe);
                    }
                    else
                    {
                        _logger.LogError("{NOMBRE_LOGGER} Error al obtener la Experiencia Educativa.", NOMBRE_LOGGER);
                        TempData["Error"] = "Error al obtener la Experiencia Educativa.";
                        return Json(null);
                    }
                }
                else
                {
                    _logger.LogError("{NOMBRE_LOGGER} Error al obtener la Experiencia Educativa.", NOMBRE_LOGGER);
                    TempData["Error"] = "Error al obtener la Experiencia Educativa.";
                    return Json(null);
                }
            }
            else
            {
                _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias Educativas.", NOMBRE_LOGGER);
                TempData["Error"] = "Error al obtener la lista de Experiencias Educativas.";
                return Json(null);
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

        [HttpGet]
        public async Task<IActionResult> CancelarAccionAsync()
        {
            var rutaArchivo = HttpContext.Session.GetString("Ruta");
            if (!string.IsNullOrEmpty(rutaArchivo))
                System.IO.File.Delete(rutaArchivo);

            var url = Url.Action(nameof(Index));

            return Json(new { Url = url });
        }

        [HttpPost]
        public async Task<IActionResult> GuardarPlanEstudiosEditadoAsync(EditarPlanViewModel modelo)
        {
            try
            {
                var plan = new EditarPlanEstudiosDTO
                {
                    IdPlanEstudios = modelo.IdPlanEstudios,
                    NuevaLista = modelo.NuevoArchivo,
                    ExperienciasNuevas = JsonSerializer.Deserialize<List<AgregarExperienciaEducativaDTO>>(HttpContext.Session.GetString("EeNuevas")),
                    ExperienciasEditadas = JsonSerializer.Deserialize<List<DatosExperienciaEducativaDTO>>(HttpContext.Session.GetString("EeEditadas")),
                    IdsExperienciasEliminadas = JsonSerializer.Deserialize<List<int>>(HttpContext.Session.GetString("EeEliminadas"))
                };

                if (modelo.NuevoArchivo)
                {
                    var rutaArchivo = HttpContext.Session.GetString("Ruta");
                    var nombreArchivo = HttpContext.Session.GetString("NombreArchivo");
                    if (!string.IsNullOrEmpty(rutaArchivo) && !string.IsNullOrEmpty(nombreArchivo))
                    {
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
                    else
                    {
                        _logger.LogError("{NOMBRE_LOGGER} La ruta o el nombre del nuevo archivo son nulos.", NOMBRE_LOGGER);
                        TempData["Error"] = "La ruta o el nombre del nuevo archivo son nulos.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    await _planEstudiosService.EditarAsync(plan);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<JsonResult> CargarNuevoArchivo(IFormFile archivo)
        {
            await guardarArchivoTemporalmente(archivo);

            var ruta = HttpContext.Session.GetString("Ruta");
            if (ruta != null)
            {
                var nombreArchivo = HttpContext.Session.GetString("NombreArchivo");
                if (nombreArchivo != null)
                {
                    var archivoDTO = new ArchivoPlanEstudiosDTO
                    {
                        Ruta = ruta,
                        NombreArchivo = nombreArchivo
                    };
                    var listaEeNueva = await ProcesarArchivoAsync(archivoDTO);
                    
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
                                if (!eeEliminadas.Contains(ee.IdExperienciaEducativa))
                                    eeEliminadas.Add(ee.IdExperienciaEducativa);
                            }
                            HttpContext.Session.SetString("EeEliminadas", JsonSerializer.Serialize(eeEliminadas));
                            HttpContext.Session.SetString("EeEditadas", JsonSerializer.Serialize(new List<DatosExperienciaEducativaDTO>()));
                            HttpContext.Session.SetString("Experiencias", JsonSerializer.Serialize(listaEeNueva));

                            return Json(listaEeNueva);
                        }
                        else
                        {
                            _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias eliminadas.", NOMBRE_LOGGER);
                            TempData["Error"] = "Error al obtener la lista de Experiencias eliminadas.";
                            return Json(null);
                        }
                    }
                    else
                    {
                        _logger.LogError("{NOMBRE_LOGGER} Error al obtener la lista de Experiencias Educativas.", NOMBRE_LOGGER);
                        TempData["Error"] = "Error al obtener la lista de Experiencias Educativas.";
                        return Json(null);
                    }
                }
                else
                {
                    _logger.LogError("{NOMBRE_LOGGER} El nombre del archivo es nulo.", NOMBRE_LOGGER);
                    TempData["Error"] = "El nombre del archivo es nulo.";
                    return Json(null);
                }
            }
            else
            {
                _logger.LogError("{NOMBRE_LOGGER} La ruta es nula.", NOMBRE_LOGGER);
                TempData["Error"] = "La ruta es nula.";
                return Json(null);
            }
        }

        /*
         * Utils
         */

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
                                        Url = Url.Action("EditarPlan", "PlanesEstudios", new { idPlanEstudios = plan.IdPlanEstudios})
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
                TempData["Error"] = "Error al cargar los Planes de Estudios.";

                return new TableModel();
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
            catch(Exception ex)
            {
                _logger.LogError(ex, "{NOMBRE_LOGGER} Error al generar la tabla de Experiencias Educativas.", NOMBRE_LOGGER);
                TempData["Error"] = "Error al generar la tabla de Experiencias Educativas.";

                return new TableModel();
            }
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

        private TableModel LlenarTablaGestionExperiencias(List<DatosExperienciaEducativaDTO> experiencias, bool edicion)
        {
            var tabla = new TableModel();
            
            if (edicion)
            {
                tabla = tabla = new TableModel
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
                        new TableCellModel()
                        {
                            Actions = new List<TableActionModel>
                            {
                                new()
                                {
                                    Accion = "editar",
                                    OnClick = $"abrirModalEditarExperiencia(true, '{ee.Codigo}', '{ee.Nombre}', '{ee.PerfilDocente}', {ee.IdExperienciaEducativa})"
                                },
                                new()
                                {
                                    Accion = "eliminar",
                                    OnClick = $"abrirModalEliminarExperiencia(true, '{ee.IdExperienciaEducativa}')"
                                }
                            }
                        }
                    }
                    }).ToList()
                };
            }
            else
            {
                tabla = new TableModel
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
                        new TableCellModel()
                        {
                            Actions = new List<TableActionModel>
                            {
                                new()
                                {
                                    Accion = "editar",
                                    OnClick = $"abrirModalEditarExperiencia(false, '{ee.Codigo}', '{ee.Nombre}', '{ee.PerfilDocente}', 0)"
                                },
                                new()
                                {
                                    Accion = "eliminar",
                                    OnClick = $"abrirModalEliminarExperiencia(false, '{ee.Codigo}')"
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
    }
}
