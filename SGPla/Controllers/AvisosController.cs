using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SGPla.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Aviso;
using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Models.DTOs.Plantillas;
using SGPla.Models.DTOs.Horario;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.PlanEstudios;
using Microsoft.IdentityModel.Tokens;
using SGPla.Commons.Factories;
using SGPla.Models.ViewModels.Avisos;
using SGPla.Services.Interfaces;
using SGPla.Views.Avisos;
using System.ComponentModel;
using System.Text.Json;

namespace SGPla.Controllers
{
    [Authorize]
    public class AvisosController : Controller
    {
        private readonly IAvisoService _avisoService;
        private readonly IPeriodoEscolarService _periodoService;
        private readonly IEntidadAcademicaService _entidadService;
        private readonly IArchivoService _archivoService;
        private readonly IArticuloService _articuloService;
        private readonly ILogger<AvisosController> _logger;
        private readonly IPlantillaService _plantillaService;
        private int _paginaActual = 1;
        private int idEntidadAcademica => int.TryParse(User.FindFirstValue("EntidadAcademicaId"), out var id) && id > 0
            ? id : throw new ValidacionExcepction("La cuenta no tiene una entidad académica asignada.", "403");
        private readonly IAvisoRepository _avisos;
        private readonly ICoordinadorDgaaRepository _coordinadores;

        private const string SESSION_HORARIOS_AGREGADOS = "HorariosAgregados";

        private const string NOMBRE_LOGGER = "AVISOS-FRONT-";
        private const string INDEX = "index:";

        public AvisosController(
            IAvisoService avisoService,
            IPeriodoEscolarService periodoService,
            IArticuloService articuloService,
            IEntidadAcademicaService entidadService,
            IArchivoService archivoService,
            IPlantillaService plantillaService,
            ILogger<AvisosController> logger, IAvisoRepository avisos, ICoordinadorDgaaRepository coordinadores)
        {
            _avisos = avisos;
            _coordinadores = coordinadores;
            _avisoService = avisoService;
            _periodoService = periodoService;
            _articuloService = articuloService;
            _entidadService = entidadService;
            _plantillaService = plantillaService;
            _archivoService = archivoService;
            _logger = logger;
        }

        // ==========
        // INDEX
        // ==========

        //Vista
        [Authorize(Policy = PoliticasAutorizacion.OperadorAcademico)]
        public async Task<IActionResult> Index(string? busqueda, int? idPeriodo, int? idEntidadAcademica, DateOnly? fechaInicio, DateOnly? fechaFin, int cantidad = 10, int pagina = 1)
        {
            try
            {
                var periodos = await generarCatalogoPeriodosAsync(idPeriodo);
                var entidades = new List<OptionModel>();
                var avisos = new List<ListaAvisosDTO>();
                var total = 0;
                int? idAreaAcademica = null;
                var rol = User.FindFirstValue(ClaimTypes.Role) ?? "";
                if (User.IsInRole(Constantes.COORDINADOR_EA))
                    idEntidadAcademica = this.idEntidadAcademica;
                else if (User.IsInRole(Constantes.COORDINADOR_DGAA))
                {
                    idAreaAcademica = await ObtenerAreaActualAsync();
                    if (idAreaAcademica is null) return Forbid();
                    entidades = await generarCatalogoEntidadesAsync(idEntidadAcademica, idAreaAcademica.Value);
                }
                else return Forbid();

                (avisos, total) = await _avisoService.ObtenerTodosAvisosAsync(new FiltroAvisosDTO
                {
                    Busqueda = busqueda,
                    IdEntidadAcademica = idEntidadAcademica,
                    IdPeriodo = idPeriodo,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    IdAreaAcademica = idAreaAcademica,
                    SoloEnviadosDgaa = User.IsInRole(Constantes.COORDINADOR_DGAA),
                    Cantidad = cantidad,
                    Pagina = pagina
                });

                TabAvisosViewModel todos = new() { Lista = avisos.Where(a => !a.Archivado).ToList() };
                TabAvisosViewModel creados = new() { Lista = avisos.Where(a => a.Estado == Constantes.CREADO && !a.Archivado).ToList() };
                TabAvisosViewModel enRevision = new() { Lista = avisos.Where(a => a.Estado == Constantes.EN_REVISION_POR_DGAA && !a.Archivado).ToList() };
                TabAvisosViewModel avalados = new() { Lista = avisos.Where(a => a.Estado == Constantes.AVALADO_POR_DGAA && !a.Archivado).ToList() };
                TabAvisosViewModel devueltos = new() { Lista = avisos.Where(a => a.Estado == Constantes.DEVUELTO_POR_DGAA && !a.Archivado).ToList() };
                TabAvisosViewModel firmados = new() { Lista = avisos.Where(a => a.Estado == Constantes.FIRMADO && !a.Archivado).ToList() };
                TabAvisosViewModel publicados = new() { Lista = avisos.Where(a => a.Estado == Constantes.PUBLICADO && !a.Archivado).ToList() };
                TabAvisosViewModel conActa = new() { Lista = avisos.Where(a => a.Estado == Constantes.ACTA_DE_CT_CREADA && !a.Archivado).ToList() };
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
        [Authorize(Policy = PoliticasAutorizacion.Dgaa)]
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

        [HttpGet]
        [Authorize(Policy = PoliticasAutorizacion.OperadorAcademico)]
        public async Task<IActionResult> VistaPreviaAvisoAsync(int idAviso)
        {
            try
            {
                var aviso = await _avisoService.ObtenerAvisoPorIDAsync(idAviso);
                if (!await PuedeConsultarAsync(idAviso))
                    return Forbid();

                if (aviso.IdArchivoFirmado <= 0 && aviso.IdArchivoOriginal <= 0)
                    return NotFound("El aviso no tiene un documento original disponible.");

                return View(new VistaPreviaAvisoViewModel
                {
                    Folio = aviso.Folio,
                    UrlVistaPrevia = Url.Action("ObtenerVistaPreviaAviso", new { idAviso })!,
                    UrlDescarga = Url.Action("DescargarAviso", new { idAviso })!
                });
            }
            catch (ValidacionExcepction)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Authorize(Policy = PoliticasAutorizacion.OperadorAcademico)]
        public async Task<IActionResult> ObtenerVistaPreviaAvisoAsync(int idAviso)
        {
            try
            {
                var aviso = await _avisoService.ObtenerAvisoPorIDAsync(idAviso);
                if (!await PuedeConsultarAsync(idAviso))
                    return Forbid();

                var archivo = await _archivoService.ObtenerVistaPreviaPdfAsync(aviso.IdArchivoFirmado > 0 ? aviso.IdArchivoFirmado : aviso.IdArchivoOriginal);
                return PhysicalFile(archivo.Ruta, archivo.Tipo, enableRangeProcessing: true);
            }
            catch (ValidacionExcepction)
            {
                return NotFound();
            }
            catch (FileNotFoundException)
            {
                return NotFound("No se encontró el documento del aviso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo generar la vista previa del aviso {IdAviso}.", idAviso);
                return Problem("No se pudo generar la vista previa del aviso.");
            }
        }

        [HttpGet]
        [Authorize(Policy = PoliticasAutorizacion.OperadorAcademico)]
        public async Task<IActionResult> DescargarAvisoAsync(int idAviso)
        {
            try
            {
                var aviso = await _avisoService.ObtenerAvisoPorIDAsync(idAviso);
                if (!await PuedeConsultarAsync(idAviso))
                    return Forbid();

                var archivo = await _archivoService.DescargarAsync(aviso.IdArchivoFirmado > 0 ? aviso.IdArchivoFirmado : aviso.IdArchivoOriginal);
                return PhysicalFile(archivo.Ruta, archivo.Tipo, archivo.Nombre);
            }
            catch (ValidacionExcepction)
            {
                return NotFound();
            }
            catch (FileNotFoundException)
            {
                return NotFound("No se encontró el documento del aviso.");
            }
        }

        // El CEA adjunta el PDF previamente firmado y registra su publicación.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(22 * 1024 * 1024)]
        [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
        public async Task<IActionResult> FirmarAvisoAsync([FromForm] int idAviso, [FromForm] IFormFile? archivo)
        {
            string? temporal = null;
            try
            {
                if (!await PuedeConsultarAsync(idAviso)) return Forbid();
                var aviso = await _avisos.ObtenerPorIDAsync(idAviso);
                if (aviso!.Archivado == true || aviso.Estado != Constantes.AVALADO_POR_DGAA)
                    return Conflict(new { message = "Solo puede firmar un aviso avalado por DGAA y no archivado." });
                if (archivo is null || archivo.Length == 0 || archivo.Length > 20 * 1024 * 1024 ||
                    !string.Equals(Path.GetExtension(archivo.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Seleccione un PDF firmado, de hasta 20 MB y no vacío." });
                await using (var stream = archivo.OpenReadStream())
                {
                    var encabezado = new byte[5];
                    var leidos = await stream.ReadAtLeastAsync(encabezado, 5, throwOnEndOfStream: false);
                    if (leidos < 5 || System.Text.Encoding.ASCII.GetString(encabezado) != "%PDF-")
                        return BadRequest(new { message = "El archivo seleccionado no tiene un formato PDF válido." });
                }
                var (nombre, ruta) = await _archivoService.GuardarTemporalmenteAsync(archivo);
                temporal = ruta;
                await _avisoService.FirmarAvisoAsync(idAviso, new CargarArchivoDTO { NombreArchivo = nombre, RutaArchivo = ruta });
                TempData["Success"] = "PDF firmado guardado. El aviso está listo para publicar.";
                return Ok(new { message = "Aviso firmado con éxito." });
            }
            catch (ValidacionExcepction ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo firmar el aviso {IdAviso}", idAviso);
                return StatusCode(500, new { message = "No se pudo guardar el PDF firmado. Inténtelo nuevamente." });
            }
            finally
            {
                if (temporal is not null)
                {
                    try { System.IO.File.Delete(temporal); }
                    catch (Exception ex) { _logger.LogWarning(ex, "No se pudo limpiar el archivo temporal de firma."); }
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
        public async Task<IActionResult> PublicarAvisoAsync(int idAviso, string url)
        {
            try
            {
                if (!await PuedeConsultarAsync(idAviso)) return Forbid();
                var aviso = await _avisos.ObtenerPorIDAsync(idAviso);
                if (aviso!.Archivado == true || aviso.Estado != Constantes.FIRMADO || aviso.IdArchivoFirmado is null)
                    return Conflict(new { message = "Solo puede publicar un aviso firmado y no archivado." });
                await _avisoService.PublicarAvisoAsync(idAviso, url);
                TempData["Success"] = "Publicación del aviso registrada con éxito.";
                return Ok(new { message = "Aviso publicado." });
            }
            catch (ValidacionExcepction ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo publicar el aviso {IdAviso}", idAviso);
                return StatusCode(500, new { message = "No se pudo registrar la publicación. Inténtelo nuevamente." });
            }
        }

        //Archivar / Desarchivar
        [HttpPost]
        [Authorize(Policy = PoliticasAutorizacion.Dgaa)]
        public async Task ArchivarAvisoAsync(int idAviso)
        {
            try
            {
                await _avisoService.ArchivarAvisoAsync(idAviso);
                TempData["Success"] = "Aviso archivado.";
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

        [HttpPost]
        [Authorize(Policy = PoliticasAutorizacion.Dgaa)]
        public async Task DesarchivarAvisoAsync(int idAviso)
        {
            try
            {
                await _avisoService.DesarchivarAvisoAsync(idAviso);
                TempData["Success"] = "Aviso desarchivado.";
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

        private async Task<int?> ObtenerAreaActualAsync()
        {
            var correo = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(correo)) return null;
            var area = (await _coordinadores.ObtenerPorCorreoAsync(correo))?.IdAreaAcademica;
            return area > 0 ? area : null;
        }

        private async Task<bool> PuedeConsultarAsync(int idAviso)
        {
            var aviso = await _avisos.ObtenerPorIDAsync(idAviso);
            if (aviso is null) return false;
            if (User.IsInRole(Constantes.COORDINADOR_EA))
                return int.TryParse(User.FindFirstValue("EntidadAcademicaId"), out var entidad)
                    && entidad > 0 && aviso.IdEntidadAcademica == entidad;
            return User.IsInRole(Constantes.COORDINADOR_DGAA)
                && EstadosAviso.RecibidosDgaa.Contains(aviso.Estado)
                && aviso.IdEntidadAcademicaNavigation.IdAreaAcademica == await ObtenerAreaActualAsync();
        }

        [HttpGet]
        [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
        public async Task<IActionResult> EnviarARevisionAsync(int idAviso)
        {
            if (!await PuedeConsultarAsync(idAviso)) return Forbid();
            var aviso = await _avisos.ObtenerPorIDAsync(idAviso);
            if (aviso!.Archivado == true || (aviso.Estado != Constantes.CREADO && aviso.Estado != Constantes.DEVUELTO_POR_DGAA))
                return Conflict("El aviso ya no está disponible para envío.");
            return View("Revision", aviso);
        }

        [HttpGet]
        [Authorize(Policy = PoliticasAutorizacion.Dgaa)]
        public async Task<IActionResult> RevisarAvisoAsync(int idAviso)
        {
            if (!await PuedeConsultarAsync(idAviso)) return Forbid();
            var aviso = await _avisos.ObtenerPorIDAsync(idAviso);
            if (aviso!.Estado != Constantes.EN_REVISION_POR_DGAA || aviso.Archivado == true) return Forbid();
            return View("Revision", aviso);
        }

        [HttpGet]
        [Authorize(Policy = PoliticasAutorizacion.Dgaa)]
        public async Task<IActionResult> EditarRevisionAsync(int idAviso)
        {
            if (!await PuedeConsultarAsync(idAviso)) return Forbid();
            var aviso = await _avisos.ObtenerPorIDAsync(idAviso);
            if (!EstadosAviso.RevisadosDgaa.Contains(aviso!.Estado)) return Forbid();
            ViewData["EditarComentarios"] = true;
            ViewData["ComentariosIngresados"] = aviso.Comentarios;
            return View("Revision", aviso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = PoliticasAutorizacion.Dgaa)]
        public Task<IActionResult> GuardarComentariosRevisionAsync(int idAviso, string comentarios)
            => GuardarRevisionAsync(new RevisionDTO { IdAviso = idAviso, Comentarios = comentarios }, true, true);

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
        public Task<IActionResult> ConfirmarEnviarARevisionAsync(RevisionDTO revision)
            => GuardarRevisionAsync(revision, false);

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = PoliticasAutorizacion.Dgaa)]
        public Task<IActionResult> ConfirmarRevisionAsync(RevisionDTO revision)
            => GuardarRevisionAsync(revision, true);

        private async Task<IActionResult> GuardarRevisionAsync(RevisionDTO revision, bool revisar, bool editarComentarios = false)
        {
            if (!await PuedeConsultarAsync(revision.IdAviso)) return Forbid();
            var aviso = await _avisos.ObtenerPorIDAsync(revision.IdAviso);
            if (revisar && (editarComentarios
                ? !EstadosAviso.RevisadosDgaa.Contains(aviso!.Estado)
                : aviso!.Estado != Constantes.EN_REVISION_POR_DGAA || aviso.Archivado == true)) return Forbid();
            try
            {
                if (!ModelState.IsValid)
                    throw new ValidacionExcepction("Revise los datos ingresados.", "400");
                if (editarComentarios) await _avisoService.EditarComentariosRevisionAsync(revision.IdAviso, revision.Comentarios);
                else if (revisar) await _avisoService.RevisarAvisoAsync(revision);
                else await _avisoService.EnviarARevisionAsync(revision);
                TempData["Success"] = editarComentarios ? "Comentarios de la revisión actualizados." : revisar
                    ? (revision.Aprobado ? "Aviso avalado por DGAA." : "Aviso devuelto a la Entidad Académica para corrección.")
                    : "El aviso se ha enviado a revisión por DGAA.";
                return RedirectToAction(nameof(Index));
            }
            catch (ValidacionExcepction ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo guardar la revisión del aviso {IdAviso}", revision.IdAviso);
                ModelState.AddModelError("", "No se pudo guardar el cambio. Inténtelo nuevamente.");
            }
            ViewData["EditarComentarios"] = editarComentarios;
            ViewData["ComentariosIngresados"] = revision.Comentarios;
            return View("Revision", await _avisos.ObtenerPorIDAsync(revision.IdAviso));
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

        [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
        public async Task<IActionResult> CrearAviso()
        {
            guardarEnSession(ObtenerLlaveHorarios(null), new List<CrearHorarioAvisoDTO>());
            return View(await ObtenerModelo());
        }

        [HttpGet]
        [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
        public async Task<IActionResult> EditarAviso(int idAviso)
        {
            try
            {
                var aviso = await _avisoService.ObtenerAvisoPorIDAsync(idAviso);
                if (aviso.IdEntidadAcademica != idEntidadAcademica ||
                    (await _avisoService.VerificarEstadoAvisoAsync(idAviso, Constantes.CREADO) == false &&
                     await _avisoService.VerificarEstadoAvisoAsync(idAviso, Constantes.DEVUELTO_POR_DGAA) == false))
                {
                    TempData["Error"] = "El Aviso no está disponible para edición.";
                    return RedirectToAction(nameof(Index));
                }

                guardarEnSession(ObtenerLlaveHorarios(idAviso), aviso.Horarios.Select(h => new CrearHorarioAvisoDTO
                {
                    IdAviso = idAviso,
                    Fecha = h.Dia,
                    HoraInicio = h.HoraInicio.ToString(@"hh\:mm"),
                    HoraTermino = h.HoraFin.ToString(@"hh\:mm")
                }).ToList());

                return View("CrearAviso", await ObtenerModelo(idAviso));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo cargar el aviso {IdAviso} para edición.", idAviso);
                TempData["Error"] = "Ha ocurrido un error, inténtelo de nuevo más tarde.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
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
                    OfertasId = model.OfertasId,
                    Horarios = obtenerDeSession<List<CrearHorarioAvisoDTO>>(ObtenerLlaveHorarios(null)) ?? []

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
        public async Task<IActionResult> EditarAviso(CrearAvisoViewModel model)
        {
            if (model.IdAviso is null || model.IdAviso <= 0)
                return RedirectToAction(nameof(Index));

            var horarios = obtenerDeSession<List<CrearHorarioAvisoDTO>>(ObtenerLlaveHorarios(model.IdAviso)) ?? [];
            if (!validarFormulario(model, horarios))
                return View("CrearAviso", await CargarCamposCrearAviso(model));

            try
            {
                await _avisoService.ActualizarAvisoPorId(new EditarAvisoDTO
                {
                    IdAviso = model.IdAviso.Value,
                    IdEntidadAcademica = idEntidadAcademica,
                    IdPeriodo = model.IdPeriodo!.Value,
                    IdArticulo = model.IdArticulo!.Value,
                    Folio = model.Folio,
                    FechaCreacion = DateOnly.FromDateTime(DateTime.Now),
                    FechaCT = DateOnly.Parse(model.FechaCT),
                    FechaVacantes = DateOnly.Parse(model.FechaVacantes),
                    Requisitos = model.Requisitos,
                    Lugar = model.Lugar,
                    Correo = model.Correo,
                    Modalidad = model.Modalidad,
                    Sistema = "Escolarizado",
                    OfertasId = model.OfertasId,
                    Horarios = horarios
                });

                HttpContext.Session.Remove(ObtenerLlaveHorarios(model.IdAviso));
                TempData["Success"] = "Aviso guardado con éxito";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo guardar el aviso {IdAviso}.", model.IdAviso);
                TempData["Error"] = ex is ValidacionExcepction ? ex.Message : "No se pudo guardar el Aviso, inténtelo de nuevo más tarde";
                return View("CrearAviso", await CargarCamposCrearAviso(model));
            }
        }

        private bool validarFormulario(CrearAvisoViewModel model, List<CrearHorarioAvisoDTO>? horarios = null)
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

            horarios ??= obtenerDeSession<List<CrearHorarioAvisoDTO>>(ObtenerLlaveHorarios(model.IdAviso));
            if ((horarios.IsNullOrEmpty()) || (horarios.Count == 0))
            {
                TempData["Warning"] = "Seleccione por lo menos un Horario";
                return false;
            }
            
            var ofertas = model.OfertasId;
            if ((ofertas.IsNullOrEmpty()) || (ofertas.Count == 0))
            {
                TempData["Warning"] = "No hay ofertas seleccionadas";
                return false;
            }
            return true;
        }

        private async Task<CrearAvisoViewModel> CargarCamposCrearAviso (CrearAvisoViewModel model)
        {
            CrearAvisoViewModel nuevoModelo = await ObtenerModelo(model.IdAviso);

            nuevoModelo.Lugar = model.Lugar ?? nuevoModelo.Lugar;
            nuevoModelo.Correo = model.Correo ?? nuevoModelo.Correo;
            nuevoModelo.FechaCT = model.FechaCT ?? nuevoModelo.FechaCT;
            
            nuevoModelo.FechaVacantes = model.FechaVacantes ?? nuevoModelo.FechaVacantes;
            nuevoModelo.Horarios = model.Horarios ?? nuevoModelo.Horarios;
            nuevoModelo.IdPeriodo = model.IdPeriodo ?? nuevoModelo.IdPeriodo;
            nuevoModelo.Modalidad = model.Modalidad ?? nuevoModelo.Modalidad;
            nuevoModelo.IdArticulo = model.IdArticulo ?? nuevoModelo.IdArticulo;
            nuevoModelo.Folio = model.Folio ?? nuevoModelo.Folio;
            nuevoModelo.IdAviso = model.IdAviso;
            nuevoModelo.Requisitos = model.Requisitos ?? nuevoModelo.Requisitos;
            nuevoModelo.OfertasId = model.OfertasId;
            nuevoModelo.PlanesEstudios = await CargarPlanesEstudios(model.IdPeriodo ?? -1, model.IdArticulo ?? -1, model.OfertasId);

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
                IdAviso = aviso.IdAviso > 0 ? aviso.IdAviso : null,
                IdPeriodo = aviso.IdPeriodo > 0 ? aviso.IdPeriodo : null,
                IdArticulo = aviso.IdArticulo > 0 ? aviso.IdArticulo : null,
                Periodos = periodosCombo,
                Articulos = articulosCombo,
                Modalidades = modalidadesCombo,
                Folio = aviso.Folio,
                FechaCT = aviso.FechaCT,
                FechaVacantes = aviso.FechaVacantes,
                Requisitos = aviso.Requisitos,
                Lugar = aviso.Lugar,
                Correo = aviso.Correo,
                Modalidad = aviso.Modalidad,
                OfertasId = aviso.Ofertas.Select(o => o.IdOferta).ToList(),
                TablaHorario = await LlenarTablaHorario(aviso.Horarios),
                PlanesEstudios = aviso.IdAviso > 0
                    ? await CargarPlanesEstudios(aviso.IdPeriodo, aviso.IdArticulo, aviso.Ofertas.Select(o => o.IdOferta).ToList())
                    : []
            };
        }

        private async Task<List<PlanEstudiosAvisoViewModel>> CargarPlanesEstudios(int idPeriodo, int idArticulo, List<int>? ofertasSeleccionadas = null)
        {
            var planes = await _avisoService.ObtenerPlanesConOfertasAviso //Actualmente retorna las ofertas por Programa educativo
                (idEntidadAcademica, idPeriodo, idArticulo); //TODO

            List<PlanEstudiosAvisoViewModel> planesViewModel = new List<PlanEstudiosAvisoViewModel>();
            foreach (var p in planes)
            {
                planesViewModel.Add(new PlanEstudiosAvisoViewModel
                {
                    Nombre = p.Nombre,
                    Tabla = await LlenarTablaOfertas(p.Ofertas),
                    Ofertas = p.Ofertas,
                    OfertasSeleccionadas = ofertasSeleccionadas ?? []
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
                                        OnClick = $"abrirModalHorario({JsonSerializer.Serialize(o)})"
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
                                        OnClick = $"verPerfilDocenteOferta('{o.PerfilDocente}')"
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
                                        OnClick = $"eliminarHorario({h.Dia}, {h.HoraInicio}, {h.HoraFin})"
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
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
        public async Task<IActionResult> ActualizarOfertasAsync(int idPeriodo, int idArticulo, [FromQuery] List<int>? ofertasSeleccionadas)
        {
            var ofertas = await CargarPlanesEstudios(idPeriodo, idArticulo, ofertasSeleccionadas);
            return PartialView("_TablasOfertas", ofertas);
        }

        [HttpGet]
        [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
        public IActionResult ObtenerHorarios(int? idAviso)
        {
            var horarios = obtenerDeSession<List<CrearHorarioAvisoDTO>>(ObtenerLlaveHorarios(idAviso));
            horarios ??= new List<CrearHorarioAvisoDTO>();
            return Json(horarios);
        }

        [HttpPost]
        [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
        public async Task<IActionResult> AgregarHorario([FromBody] List<CrearHorarioAvisoDTO> horarios, int? idAviso)
        {
            guardarEnSession(ObtenerLlaveHorarios(idAviso), horarios);
            var tabla = await ActualizarTablaHorarios(horarios);
            return PartialView("_Horarios", tabla);
        }

        [HttpGet]
        [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
        public async Task<IActionResult> ObtenerTablaHorarios(int? idAviso)
        {
            var horarios = obtenerDeSession<List<CrearHorarioAvisoDTO>>(ObtenerLlaveHorarios(idAviso));

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
                                        AriaLabel = "Editar horario",
                                        OnClick = $"editarHorario({JsonSerializer.Serialize(h.Fecha)}, {JsonSerializer.Serialize(h.HoraInicio)}, {JsonSerializer.Serialize(h.HoraTermino)})"
                                    },
                                    new TableActionModel
                                    {
                                        Accion = "Eliminar",
                                        AriaLabel = "Eliminar horario",
                                        OnClick = $"solicitarEliminarHorario({JsonSerializer.Serialize(h.Fecha)}, {JsonSerializer.Serialize(h.HoraInicio)}, {JsonSerializer.Serialize(h.HoraTermino)})"
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

        private static string ObtenerLlaveHorarios(int? idAviso) =>
            $"{SESSION_HORARIOS_AGREGADOS}:{(idAviso?.ToString() ?? "nuevo")}";

        private void guardarEnSession<T>(string llave, T objeto)
        {
            var json = JsonSerializer.Serialize(objeto);
            HttpContext.Session.SetString(llave, json);
        }
    }
}
