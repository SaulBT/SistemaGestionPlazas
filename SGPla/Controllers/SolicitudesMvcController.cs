using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models.DTOs.Solicitudes;
using SGPla.Models.ViewModels.Solicitudes;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace SGPla.Controllers;

[Route("Avisos/{avisoId:int}/Solicitudes")]
[Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
public sealed class SolicitudesMvcController : Controller
{
    private readonly ISolicitudMvcService _service;

    public SolicitudesMvcController(ISolicitudMvcService service) => _service = service;

    [HttpGet("")]
    public async Task<IActionResult> Index(int avisoId, CancellationToken cancellationToken)
    {
        if (!ObtenerActor(out var usuarioId, out var entidadId)) return Forbid();
        SolicitudMvcListado? datos;
        try { datos = await _service.ListarPorAvisoAsync(usuarioId, entidadId, avisoId, cancellationToken); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        if (datos is null) return NotFound();
        return View("Index", new SolicitudMvcListadoViewModel
        {
            AvisoId = datos.AvisoId,
            EstadoAviso = datos.EstadoAviso,
            Solicitudes = datos.Solicitudes,
            GradosAcademicos = datos.GradosAcademicos,
            VacantesDisponibles = datos.VacantesDisponibles,
            TiposDocumento = datos.TiposDocumento
        });
    }

    [HttpGet("Nueva/{avisoOfertaId:int}")]
    public async Task<IActionResult> Nueva(int avisoId, int avisoOfertaId, CancellationToken cancellationToken)
    {
        if (!ObtenerActor(out var usuarioId, out var entidadId)) return Forbid();
        SolicitudMvcListado? datos;
        try { datos = await _service.ListarPorAvisoAsync(usuarioId, entidadId, avisoId, cancellationToken); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        if (datos is null) return NotFound();
        var vacante = datos.VacantesDisponibles.SingleOrDefault(x => x.AvisoOfertaId == avisoOfertaId);
        if (vacante is null) return Conflict("La recepción está cerrada o la vacante no está disponible.");
        return View("Nueva", new SolicitudMvcRegistroViewModel
        {
            AvisoId = avisoId,
            AvisoOfertaId = avisoOfertaId,
            Vacante = $"{vacante.ClavePlaza} · NRC {vacante.Nrc} · {vacante.Materia} {vacante.Curso}",
            GradosAcademicos = datos.GradosAcademicos,
            Formaciones = [new FormacionSolicitudMvcViewModel()]
        });
    }

    [HttpPost("Nueva/{avisoOfertaId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nueva(int avisoId, int avisoOfertaId, SolicitudMvcRegistroViewModel modelo,
        CancellationToken cancellationToken)
    {
        if (!ObtenerActor(out var usuarioId, out var entidadId)) return Forbid();
        modelo.AvisoId = avisoId;
        modelo.AvisoOfertaId = avisoOfertaId;
        if (!ModelState.IsValid)
        {
            await RecargarFormularioAsync(modelo, usuarioId, entidadId, cancellationToken);
            return View("Nueva", modelo);
        }

        try
        {
            var id = await _service.RegistrarAsync(usuarioId, entidadId, new RegistrarSolicitudMvcDatos(
                avisoOfertaId, modelo.Nombre, modelo.Correo, modelo.PuestoActual, modelo.DescripcionPerfil,
                modelo.Formaciones.Select(x => new FormacionSolicitudMvcDatos(x.GradoAcademicoId, x.Descripcion)).ToList(),
                modelo.Observaciones), cancellationToken);
            TempData["Success"] = "La Solicitud se registró correctamente.";
            return RedirectToAction(nameof(Detalle), new { avisoId, solicitudId = id });
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException
                                           or Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            TempData["Error"] = exception.Message;
            await RecargarFormularioAsync(modelo, usuarioId, entidadId, cancellationToken);
            return View("Nueva", modelo);
        }
    }

    [HttpGet("{solicitudId:int}")]
    public async Task<IActionResult> Detalle(int avisoId, int solicitudId, CancellationToken cancellationToken)
    {
        if (!ObtenerActor(out var usuarioId, out var entidadId)) return Forbid();
        SolicitudMvcDetalle? datos;
        try { datos = await _service.ObtenerDetalleAsync(usuarioId, entidadId, avisoId, solicitudId, cancellationToken); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        if (datos is null) return NotFound();
        SolicitudMvcListado? listado;
        try { listado = await _service.ListarPorAvisoAsync(usuarioId, entidadId, avisoId, cancellationToken); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        var modelo = Convertir(datos, listado?.GradosAcademicos ?? []);
        modelo.TiposDocumento = listado?.TiposDocumento ?? [];
        return View("Detalle", modelo);
    }

    [HttpPost("{solicitudId:int}/Documentos")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(AlmacenDocumentosLocal.TamanoMaximo)]
    public async Task<IActionResult> AgregarDocumento(int avisoId, int solicitudId, int tipoDocumentoId,
        int? documentoAspiranteId,
        IFormFile? archivo, CancellationToken cancellationToken)
    {
        if (!ObtenerActor(out var usuarioId, out var entidadId)) return Forbid();
        if (archivo is null || archivo.Length < 1 || archivo.Length > AlmacenDocumentosLocal.TamanoMaximo)
        {
            TempData["Error"] = "Seleccione un PDF de hasta 25 MB.";
            return RedirectToAction(nameof(Detalle), new { avisoId, solicitudId });
        }
        try
        {
            await using var stream = archivo.OpenReadStream();
            await _service.AgregarDocumentoAsync(usuarioId, entidadId, avisoId, solicitudId, tipoDocumentoId, documentoAspiranteId,
                stream, archivo.FileName, archivo.ContentType, archivo.Length, cancellationToken);
            TempData["Success"] = "El documento se agregó a la Solicitud.";
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException
                                           or KeyNotFoundException
                                           or Microsoft.EntityFrameworkCore.DbUpdateException or AggregateException)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Detalle), new { avisoId, solicitudId });
    }

    [HttpGet("{solicitudId:int}/Documentos/{versionDocumentoId:int}")]
    public async Task<IActionResult> DescargarDocumento(int avisoId, int solicitudId, int versionDocumentoId,
        CancellationToken cancellationToken)
    {
        if (!ObtenerActor(out var usuarioId, out var entidadId)) return Forbid();
        DescargaDocumentoSolicitudMvc? documento;
        try
        {
            documento = await _service.AbrirDocumentoAsync(usuarioId, entidadId, avisoId, solicitudId,
                versionDocumentoId, cancellationToken);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        return documento is null ? NotFound() : File(documento.Contenido, documento.Mime, documento.Nombre);
    }

    [HttpPost("{solicitudId:int}/Perfil")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarPerfil(int avisoId, int solicitudId, SolicitudMvcDetalleViewModel modelo,
        CancellationToken cancellationToken)
    {
        if (!ObtenerActor(out var usuarioId, out var entidadId)) return Forbid();
        if (!ModelState.IsValid || modelo.Formaciones.Count is 0 or > 30)
        {
            TempData["Error"] = "Revise el perfil y capture entre una y treinta formaciones.";
            return RedirectToAction(nameof(Detalle), new { avisoId, solicitudId });
        }
        try
        {
            await _service.ActualizarPerfilAsync(usuarioId, entidadId, avisoId, solicitudId,
                new EditarPerfilSolicitudMvcDatos(modelo.Nombre, modelo.Correo, modelo.PuestoActual,
                    modelo.DescripcionPerfil, modelo.Formaciones.Select(x => new FormacionSolicitudMvcDatos(
                        x.GradoAcademicoId, x.Descripcion)).ToList()), cancellationToken);
            TempData["Success"] = "Se guardó una nueva versión del perfil.";
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException
                                           or UnauthorizedAccessException or KeyNotFoundException
                                           or Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Detalle), new { avisoId, solicitudId });
    }

    [HttpPost("{solicitudId:int}/Resolver")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolver(int avisoId, int solicitudId, bool admitir, string? motivoNoAdmision,
        CancellationToken cancellationToken)
    {
        if (!ObtenerActor(out var usuarioId, out var entidadId)) return Forbid();
        try
        {
            await _service.ResolverAsync(usuarioId, entidadId, avisoId, solicitudId,
                new ResolverSolicitudMvcDatos(admitir, motivoNoAdmision), cancellationToken);
            TempData["Success"] = admitir ? "La Solicitud fue admitida." : "La Solicitud fue marcada como no admitida.";
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException
                                           or KeyNotFoundException)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Detalle), new { avisoId, solicitudId });
    }

    [HttpPost("{solicitudId:int}/Retirar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Retirar(int avisoId, int solicitudId, string? motivo,
        CancellationToken cancellationToken)
    {
        if (!ObtenerActor(out var usuarioId, out var entidadId)) return Forbid();
        try
        {
            await _service.RetirarAsync(usuarioId, entidadId, avisoId, solicitudId, motivo, cancellationToken);
            TempData["Success"] = "La Solicitud se retiró antes de la sesión.";
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException
                                           or KeyNotFoundException)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Detalle), new { avisoId, solicitudId });
    }

    private async Task RecargarFormularioAsync(SolicitudMvcRegistroViewModel modelo, int usuarioId, int entidadId,
        CancellationToken cancellationToken)
    {
        var listado = await _service.ListarPorAvisoAsync(usuarioId, entidadId, modelo.AvisoId, cancellationToken);
        modelo.GradosAcademicos = listado?.GradosAcademicos ?? [];
        var vacante = listado?.VacantesDisponibles.SingleOrDefault(x => x.AvisoOfertaId == modelo.AvisoOfertaId);
        modelo.Vacante = vacante is null ? "Vacante no disponible" : $"{vacante.ClavePlaza} · NRC {vacante.Nrc}";
    }

    private static SolicitudMvcDetalleViewModel Convertir(SolicitudMvcDetalle datos,
        IReadOnlyList<GradoAcademicoSolicitudMvcOpcion> grados) => new()
    {
        AvisoId = datos.AvisoId, Id = datos.Id, AvisoOfertaId = datos.AvisoOfertaId, Estado = datos.Estado,
        Nombre = datos.Nombre, Correo = datos.Correo, PuestoActual = datos.PuestoActual,
        DescripcionPerfil = datos.DescripcionPerfil, Observaciones = datos.Observaciones,
        RegistradaEn = datos.RegistradaEn, AdmitidaEn = datos.AdmitidaEn,
        NoAdmitidaEn = datos.NoAdmitidaEn, MotivoNoAdmision = datos.MotivoNoAdmision,
        FormacionesCapturadas = datos.Formaciones, GradosAcademicos = grados, Documentos = datos.Documentos,
        Formaciones = datos.Formaciones.Select(x => new FormacionSolicitudMvcViewModel
        { GradoAcademicoId = x.GradoAcademicoId, Descripcion = x.Descripcion }).ToList()
    };

    private bool ObtenerActor(out int usuarioId, out int entidadId)
    {
        usuarioId = 0;
        entidadId = 0;
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out usuarioId) && usuarioId > 0
            && int.TryParse(User.FindFirstValue("EntidadAcademicaId"), out entidadId) && entidadId > 0;
    }
}
