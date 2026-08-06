using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Aviso;
using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Models.ViewModels.Avisos;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers
{
    public class AvisosController : Controller
    {
        private readonly IAvisoService _avisoService;
        private readonly IPeriodoEscolarService _periodoService;
        private readonly IEntidadAcademicaService _entidadService;
        private readonly IArchivoService _archivoService;
        private readonly ILogger<AvisosController> _logger;
        private int _paginaActual = 1;

        private const string SESSION_ROL = "Rol";

        private const string NOMBRE_LOGGER = "AVISOS-FRONT-";
        private const string INDEX = "index:";

        public AvisosController(
            IAvisoService avisoService,
            IPeriodoEscolarService periodoService,
            IEntidadAcademicaService entidadService,
            IArchivoService archivoService,
            ILogger<AvisosController> logger)
        {
            _avisoService = avisoService;
            _periodoService = periodoService;
            _entidadService = entidadService;
            _logger = logger;
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
    }
}
