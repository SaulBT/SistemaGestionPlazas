using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models.DTOs.Avisos;
using SGPla.Models.ViewModels.Avisos;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers;

[Route("Avisos")]
[Authorize(Policy = PoliticasAutorizacion.OperadorAcademico)]
public sealed class AvisosMvcController : Controller
{
    private readonly IAvisoMvcService _service;
    private readonly IDocumentoAvisoMvcService _documentos;

    public AvisosMvcController(IAvisoMvcService service, IDocumentoAvisoMvcService documentos)
    {
        _service = service;
        _documentos = documentos;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int pagina = 1, int tamanoPagina = 20,
        int? periodoEscolarId = null, int? entidadAcademicaId = null, string? estado = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUsuarioId();
        if (!userId.HasValue) return Forbid();
        try
        {
            var result = await _service.BuscarAsync(userId.Value,
                new AvisoMvcFiltro(pagina, tamanoPagina, periodoEscolarId, entidadAcademicaId, estado), cancellationToken);
            return View("Index", new AvisosMvcIndexViewModel
            {
                Avisos = result.Items, Total = result.Total, Pagina = result.Pagina,
                TamanoPagina = result.TamanoPagina, PeriodoEscolarId = periodoEscolarId,
                EntidadAcademicaId = entidadAcademicaId, Estado = estado,
                Periodos = result.Periodos, Entidades = result.Entidades
            });
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException exception) { return BadRequest(exception.Message); }
    }

    [HttpGet("Nuevo")]
    [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
    public async Task<IActionResult> Nuevo(int? periodoEscolarId = null, int? sistemaEducativoId = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUsuarioId();
        if (!userId.HasValue) return Forbid();
        try
        {
            var data = await _service.ObtenerDatosNuevoAsync(userId.Value, periodoEscolarId,
                sistemaEducativoId, cancellationToken);
            return View("Nuevo", new CrearAvisoMvcViewModel
            {
                PeriodoEscolarId = periodoEscolarId ?? 0,
                SistemaEducativoId = sistemaEducativoId ?? 0,
                Datos = data
            });
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException exception) { return BadRequest(exception.Message); }
    }

    [HttpPost("Nuevo")]
    [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nuevo(CrearAvisoMvcViewModel model, CancellationToken cancellationToken)
    {
        var userId = GetUsuarioId();
        var entidadId = GetEntidadAcademicaId();
        if (!userId.HasValue || !entidadId.HasValue) return Forbid();
        try
        {
            model.Datos = await _service.ObtenerDatosNuevoAsync(userId.Value, model.PeriodoEscolarId,
                model.SistemaEducativoId, cancellationToken);
            if (model.OfertaIds is null || model.OfertaIds.Count == 0)
                ModelState.AddModelError(nameof(model.OfertaIds), "Selecciona al menos una Oferta disponible.");
            if (!ModelState.IsValid) return View("Nuevo", model);

            var id = await _service.CrearBorradorAsync(userId.Value, entidadId.Value,
                new CrearAvisoMvcDatos(model.PeriodoEscolarId, model.SistemaEducativoId,
                    model.ArticuloId, model.TipoComunicado, model.OfertaIds ?? []), cancellationToken);
            TempData["Success"] = $"Se creó el Aviso {id} en estado CREADO.";
            return RedirectToAction(nameof(Index));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException exception)
        {
            if (model.PeriodoEscolarId <= 0 || model.SistemaEducativoId <= 0) return BadRequest(exception.Message);
            ModelState.AddModelError(string.Empty, exception.Message);
            model.Datos = await _service.ObtenerDatosNuevoAsync(userId.Value, model.PeriodoEscolarId,
                model.SistemaEducativoId, cancellationToken);
            return View("Nuevo", model);
        }
    }

    [HttpGet("{avisoId:int}")]
    public async Task<IActionResult> Detalle(int avisoId, CancellationToken cancellationToken)
    {
        var userId = GetUsuarioId();
        if (!userId.HasValue) return Forbid();
        try
        {
            var aviso = await _service.ObtenerDetalleAsync(userId.Value, avisoId, cancellationToken);
            if (aviso is null) return NotFound();
            var editable = User.IsInRole(SGPla.Commons.Constantes.COORDINADOR_EA)
                && (aviso.Estado is "CREADO" or "DEVUELTO_DGAA");
            var data = editable
                    ? await _service.ObtenerDatosNuevoAsync(userId.Value, aviso.PeriodoEscolarId,
                        aviso.SistemaEducativoId, cancellationToken)
                    : new AvisoMvcNuevoDatos([], [], [], [], []);
            return View("Detalle", new AvisoMvcDetalleViewModel
            {
                Aviso = aviso, Datos = data,
                Envio = new EnviarAvisoMvcViewModel
                {
                    ModalidadRecepcionId = aviso.ModalidadRecepcionId ?? 0,
                    Requisitos = aviso.Requisitos ?? string.Empty,
                    LugarRecepcion = aviso.LugarRecepcion,
                    CorreoContacto = aviso.CorreoContacto ?? string.Empty,
                    NombreTitular = aviso.NombreTitular ?? string.Empty,
                    FechaConsejoTecnico = aviso.FechaConsejoTecnico ?? default,
                    FechaVacantes = aviso.FechaVacantes ?? default,
                    Horarios = aviso.Horarios.Count == 0 ? [new()] : aviso.Horarios.Select(x =>
                        new AvisoHorarioEntradaViewModel { Fecha = x.Fecha, HoraInicio = x.HoraInicio, HoraFin = x.HoraFin }).ToList()
                }
            });
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost("{avisoId:int}/EnviarARevision")]
    [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarARevision(int avisoId, AvisoMvcDetalleViewModel model,
        CancellationToken cancellationToken)
    {
        var userId = GetUsuarioId();
        var entidadId = GetEntidadAcademicaId();
        if (!userId.HasValue || !entidadId.HasValue) return Forbid();
        if (model?.Envio?.Horarios is null) return BadRequest("La solicitud de envío está incompleta.");
        try
        {
            var horarios = model.Envio.Horarios.Select(x => new AvisoMvcHorario(x.Fecha, x.HoraInicio, x.HoraFin)).ToArray();
            await _service.EnviarARevisionAsync(userId.Value, entidadId.Value, avisoId,
                new ConfigurarYEnviarAvisoMvcDatos(model.Envio.ModalidadRecepcionId,
                    model.Envio.Requisitos, model.Envio.LugarRecepcion, model.Envio.CorreoContacto,
                    model.Envio.NombreTitular, model.Envio.FechaConsejoTecnico, model.Envio.FechaVacantes,
                    horarios), cancellationToken);
            TempData["Success"] = "El Aviso se envió a revisión de DGAA.";
            return RedirectToAction(nameof(Detalle), new { avisoId });
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            var aviso = await _service.ObtenerDetalleAsync(userId.Value, avisoId, cancellationToken);
            if (aviso is null) return NotFound();
            model.Aviso = aviso;
            model.Datos = await _service.ObtenerDatosNuevoAsync(userId.Value,
                aviso.PeriodoEscolarId, aviso.SistemaEducativoId, cancellationToken);
            return View("Detalle", model);
        }
    }

    [HttpPost("{avisoId:int}/ResolverRevision")]
    [Authorize(Policy = PoliticasAutorizacion.Dgaa)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResolverRevision(int avisoId, bool avalar, string? comentarios,
        CancellationToken cancellationToken)
    {
        var userId = GetUsuarioId();
        if (!userId.HasValue) return Forbid();
        try
        {
            await _service.ResolverRevisionAsync(userId.Value, avisoId, avalar, comentarios, cancellationToken);
            TempData["Success"] = avalar ? "El Aviso quedó avalado por DGAA." : "El Aviso fue devuelto a la Entidad Académica.";
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Detalle), new { avisoId });
    }

    [HttpPost("{avisoId:int}/Ofertas")]
    [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarOfertas(int avisoId, AvisoMvcDetalleViewModel model,
        CancellationToken cancellationToken)
    {
        var userId = GetUsuarioId();
        var entidadId = GetEntidadAcademicaId();
        if (!userId.HasValue || !entidadId.HasValue) return Forbid();
        try
        {
            await _service.AgregarOfertasAsync(userId.Value, entidadId.Value, avisoId, model.OfertaIds, cancellationToken);
            TempData["Success"] = "Se agregaron las Ofertas seleccionadas al borrador.";
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Detalle), new { avisoId });
    }

    [HttpPost("{avisoId:int}/Ofertas/{avisoOfertaId:int}/Retirar")]
    [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RetirarOferta(int avisoId, int avisoOfertaId, CancellationToken cancellationToken)
    {
        var userId = GetUsuarioId();
        var entidadId = GetEntidadAcademicaId();
        if (!userId.HasValue || !entidadId.HasValue) return Forbid();
        try
        {
            await _service.RetirarOfertaAsync(userId.Value, entidadId.Value, avisoId, avisoOfertaId, cancellationToken);
            TempData["Success"] = "Se retiró la Oferta; volvió a estar disponible.";
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception exception) when (exception is KeyNotFoundException or InvalidOperationException)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Detalle), new { avisoId });
    }

    [HttpPost("{avisoId:int}/DocumentoOriginal")]
    [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(AlmacenDocumentosLocal.TamanoMaximo + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = AlmacenDocumentosLocal.TamanoMaximo + 1024 * 1024)]
    public async Task<IActionResult> SubirDocumentoOriginal(int avisoId, IFormFile? archivo,
        CancellationToken cancellationToken)
    {
        var userId = GetUsuarioId();
        var entidadId = GetEntidadAcademicaId();
        if (!userId.HasValue || !entidadId.HasValue) return Forbid();
        if (archivo is null || archivo.Length <= 0)
        {
            TempData["Error"] = "Selecciona un documento PDF.";
            return RedirectToAction(nameof(Detalle), new { avisoId });
        }

        try
        {
            await using var contenido = archivo.OpenReadStream();
            await _documentos.GuardarOriginalAsync(avisoId, entidadId.Value, userId.Value,
                contenido, archivo.FileName, archivo.ContentType, archivo.Length, cancellationToken);
            TempData["Success"] = "Se guardó una nueva versión del documento original.";
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception exception) when (exception is ArgumentException or InvalidDataException or InvalidOperationException)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Detalle), new { avisoId });
    }

    [HttpGet("{avisoId:int}/Documentos/{documentoId:int}")]
    public async Task<IActionResult> DescargarDocumento(int avisoId, int documentoId,
        CancellationToken cancellationToken)
    {
        var userId = GetUsuarioId();
        if (!userId.HasValue) return Forbid();
        var descarga = await _documentos.AbrirParaDescargaAsync(avisoId, documentoId, userId.Value, cancellationToken);
        if (descarga is null) return NotFound();
        return File(descarga.Contenido, descarga.Mime, descarga.Nombre, enableRangeProcessing: true);
    }

    private int? GetUsuarioId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0 ? id : null;

    private int? GetEntidadAcademicaId() =>
        int.TryParse(User.FindFirstValue("EntidadAcademicaId"), out var id) && id > 0 ? id : null;
}
