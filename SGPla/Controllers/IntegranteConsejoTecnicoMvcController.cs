using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models.DTOs.IntegranteCt;
using SGPla.Models.ViewModels.IntegranteCt;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers;

[Route("IntegranteCT")]
[Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
public sealed class IntegranteConsejoTecnicoMvcController : Controller
{
    private readonly IIntegranteConsejoTecnicoMvcService _service;
    private readonly TimeProvider _timeProvider;

    public IntegranteConsejoTecnicoMvcController(IIntegranteConsejoTecnicoMvcService service, TimeProvider timeProvider)
    {
        _service = service;
        _timeProvider = timeProvider;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? busqueda, int pagina = 1, int tamanoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetScope(out var usuarioId, out var entidadId)) return Forbid();
        try
        {
            var result = await _service.BuscarAsync(usuarioId, entidadId,
                new IntegranteConsejoTecnicoMvcFiltro(busqueda, pagina, tamanoPagina), cancellationToken);
            return View("IndexMvc", CrearModelo(result, busqueda));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost("Crear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(IntegranteConsejoTecnicoMvcIndexViewModel model,
        CancellationToken cancellationToken)
    {
        if (!TryGetScope(out var usuarioId, out var entidadId)) return Forbid();
        if (!ModelState.IsValid)
            return await DevolverFormularioConErroresAsync(usuarioId, entidadId, model, cancellationToken);
        try
        {
            await _service.CrearAsync(usuarioId, entidadId, model.Formulario.ToDto(), cancellationToken);
            TempData["Success"] = "Se registró el integrante con su vigencia.";
            return RedirectToAction(nameof(Index));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return await DevolverFormularioConErroresAsync(usuarioId, entidadId, model, cancellationToken);
        }
    }

    [HttpPost("{integranteId:int}/NuevaVigencia")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NuevaVigencia(int integranteId,
        IntegranteConsejoTecnicoMvcIndexViewModel model, CancellationToken cancellationToken)
    {
        if (!TryGetScope(out var usuarioId, out var entidadId)) return Forbid();
        if (!ModelState.IsValid)
            return await DevolverFormularioConErroresAsync(usuarioId, entidadId, model, cancellationToken);
        try
        {
            await _service.CambiarVigenciaAsync(usuarioId, entidadId, integranteId,
                model.Formulario.ToDto(), cancellationToken);
            TempData["Success"] = "Se cerró la vigencia anterior y se registró el nuevo nombramiento.";
            return RedirectToAction(nameof(Index), new { busqueda = model.Busqueda, pagina = model.Pagina });
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or KeyNotFoundException)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return await DevolverFormularioConErroresAsync(usuarioId, entidadId, model, cancellationToken);
        }
    }

    [HttpPost("{integranteId:int}/EliminarCapturaErronea")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarCapturaErronea(int integranteId,
        CancellationToken cancellationToken)
    {
        if (!TryGetScope(out var usuarioId, out var entidadId)) return Forbid();
        try
        {
            await _service.EliminarCapturaErroneaAsync(usuarioId, entidadId, integranteId, cancellationToken);
            TempData["Success"] = "Se eliminó la captura errónea sin referencias a Actas.";
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or KeyNotFoundException)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> DevolverFormularioConErroresAsync(int usuarioId, int entidadId,
        IntegranteConsejoTecnicoMvcIndexViewModel model, CancellationToken cancellationToken)
    {
        var result = await _service.BuscarAsync(usuarioId, entidadId,
            new IntegranteConsejoTecnicoMvcFiltro(model.Busqueda, model.Pagina, model.TamanoPagina), cancellationToken);
        model.Integrantes = result.Items;
        model.Tratamientos = result.Tratamientos;
        model.Total = result.Total;
        model.Pagina = result.Pagina;
        model.TamanoPagina = result.TamanoPagina;
        return View("IndexMvc", model);
    }

    private IntegranteConsejoTecnicoMvcIndexViewModel CrearModelo(
        IntegranteConsejoTecnicoMvcPagina result, string? busqueda) => new()
    {
        Integrantes = result.Items,
        Tratamientos = result.Tratamientos,
        Total = result.Total,
        Pagina = result.Pagina,
        TamanoPagina = result.TamanoPagina,
        Busqueda = busqueda,
        Formulario = new IntegranteConsejoTecnicoMvcCambioViewModel
        {
            FechaInicio = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime)
        }
    };

    private bool TryGetScope(out int usuarioId, out int entidadId)
    {
        var hasUser = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out usuarioId) && usuarioId > 0;
        var hasEntity = int.TryParse(User.FindFirstValue("EntidadAcademicaId"), out entidadId) && entidadId > 0;
        return hasUser && hasEntity;
    }
}
