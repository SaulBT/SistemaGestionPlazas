using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models.DTOs.Ofertas;
using SGPla.Models.ViewModels.ProgramacionesAcademicas;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers;

[Route("ProgramacionesAcademicas/{programacionId:int}/Ofertas")]
[Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
public sealed class OfertasMvcController : Controller
{
    private readonly IOfertaMvcService _service;

    public OfertasMvcController(IOfertaMvcService service) => _service = service;

    [HttpGet("Nueva")]
    public async Task<IActionResult> Nueva(int programacionId, CancellationToken cancellationToken)
    {
        var scope = GetEntidadAcademicaId();
        if (!scope.HasValue) return Forbid();
        var datos = await _service.ObtenerDatosNuevaAsync(programacionId, scope.Value, cancellationToken);
        if (datos is null) return NotFound();
        return View("Nueva", CrearModelo(datos));
    }

    [HttpPost("Nueva")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nueva(int programacionId, CrearOfertaViewModel modelo, CancellationToken cancellationToken)
    {
        var scope = GetEntidadAcademicaId();
        if (!scope.HasValue) return Forbid();
        if (modelo.ProgramacionAcademicaId != programacionId)
            ModelState.AddModelError(nameof(modelo.ProgramacionAcademicaId), "La programación enviada no coincide con la ruta.");
        var datos = await _service.ObtenerDatosNuevaAsync(programacionId, scope.Value, cancellationToken);
        if (datos is null) return NotFound();
        if (!ModelState.IsValid) return View("Nueva", CrearModelo(datos, modelo));

        try
        {
            var id = await _service.CrearAsync(new CrearOfertaMvcDatos(programacionId, modelo.ClavePlaza,
                modelo.TipoPlazaId, modelo.TipoContratacionId, modelo.PerfilSolicitado, modelo.Justificacion),
                scope.Value, cancellationToken);
            TempData["Success"] = $"Se creó la Oferta {id} en estado DISPONIBLE.";
            return RedirectToAction("Index", "ProgramacionesAcademicasMvc");
        }
        catch (ArgumentException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
        catch (InvalidOperationException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
        return View("Nueva", CrearModelo(datos, modelo));
    }

    private int? GetEntidadAcademicaId() =>
        int.TryParse(User.FindFirstValue("EntidadAcademicaId"), out var id) && id > 0 ? id : null;

    private static CrearOfertaViewModel CrearModelo(OfertaMvcNuevaDatos datos, CrearOfertaViewModel? original = null) => new()
    {
        ProgramacionAcademicaId = datos.Programacion.ProgramacionAcademicaId,
        ClavePlaza = original?.ClavePlaza ?? string.Empty,
        TipoPlazaId = original?.TipoPlazaId ?? 0,
        TipoContratacionId = original?.TipoContratacionId ?? 0,
        PerfilSolicitado = original?.PerfilSolicitado ?? string.Empty,
        Justificacion = original?.Justificacion,
        Programacion = datos.Programacion,
        TiposPlaza = datos.TiposPlaza,
        TiposContratacion = datos.TiposContratacion
    };
}
