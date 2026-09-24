using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models.DTOs.ProgramacionAcademica;
using SGPla.Models.ViewModels.ProgramacionesAcademicas;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers;

[Route("ProgramacionesAcademicas")]
public sealed class ProgramacionesAcademicasMvcController : Controller
{
    private readonly IProgramacionAcademicaMvcService _service;
    private readonly ICatalogosMvcService _catalogos;
    private readonly ISincronizacionPlaneaService _sincronizacionPlanea;

    public ProgramacionesAcademicasMvcController(IProgramacionAcademicaMvcService service, ICatalogosMvcService catalogos,
        ISincronizacionPlaneaService sincronizacionPlanea)
    {
        _service = service;
        _catalogos = catalogos;
        _sincronizacionPlanea = sincronizacionPlanea;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    [Authorize(Policy = PoliticasAutorizacion.OperadorAcademico)]
    public async Task<IActionResult> Index(ProgramacionAcademicaMvcFiltro filtro, CancellationToken cancellationToken)
    {
        var scopeId = GetEntidadAcademicaAutorizadaId();
        filtro = filtro with { Pagina = Math.Max(1, filtro.Pagina), TamanoPagina = Math.Clamp(filtro.TamanoPagina, 1, 100) };
        var pagina = await _service.BuscarAsync(filtro, scopeId, cancellationToken);
        var regiones = await _catalogos.ObtenerAsync(cancellationToken);
        var entidadOpciones = scopeId.HasValue
            ? Array.Empty<Models.ViewModels.Catalogos.CatalogoOpcion>()
            : await _catalogos.ObtenerEntidadesAsync(null, null, cancellationToken, filtro.RegionId);
        return View("IndexMvc", new ProgramacionAcademicaMvcIndexViewModel
        {
            Filtro = filtro,
            Items = pagina.Items,
            Total = pagina.Total,
            Regiones = regiones.Regiones,
            Entidades = entidadOpciones,
            Programas = await _catalogos.ObtenerProgramasAsync(scopeId ?? filtro.EntidadAcademicaId, cancellationToken),
            Periodos = await _service.ObtenerPeriodosAsync(cancellationToken)
        });
    }

    [HttpGet("Importar")]
    [Authorize(Policy = PoliticasAutorizacion.Dgaa)]
    public async Task<IActionResult> Importar(CancellationToken cancellationToken) =>
        View("ImportarMvc", await PrepararImportacionAsync(new ProgramacionAcademicaImportarViewModel(), cancellationToken));

    [HttpPost("Importar")]
    [Authorize(Policy = PoliticasAutorizacion.Dgaa)]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(21 * 1024 * 1024)]
    public async Task<IActionResult> Importar(ProgramacionAcademicaImportarViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Selecciona el periodo, la Entidad Académica y ambos archivos.";
            return View("ImportarMvc", await PrepararImportacionAsync(modelo, cancellationToken));
        }
        try
        {
            var resultado = await _service.ImportarVacantesYDescargasAsync(
                modelo.PeriodoEscolarId, modelo.EntidadAcademicaId, modelo.ArchivoVacantes!, modelo.ArchivoDescargas!, cancellationToken);
            TempData["Success"] = $"Programaciones creadas: {resultado.Creadas}; ya existentes: {resultado.YaExistentes}; NRC importados: {resultado.TotalArchivo}. Los horarios y docentes se sincronizan desde PLANEA.";
            return RedirectToAction(nameof(Index), new { periodoEscolarId = modelo.PeriodoEscolarId, entidadAcademicaId = modelo.EntidadAcademicaId });
        }
        catch (ArgumentException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
        catch (InvalidOperationException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
        return View("ImportarMvc", await PrepararImportacionAsync(modelo, cancellationToken));
    }

    [HttpPost("SincronizarPlanea")]
    [Authorize(Policy = PoliticasAutorizacion.Dgaa)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SincronizarPlanea(int periodoEscolarId, CancellationToken cancellationToken)
    {
        try
        {
            var id = await _sincronizacionPlanea.SincronizarAsync(periodoEscolarId, cancellationToken);
            TempData["Success"] = $"La sincronización PLANEA {id} terminó correctamente.";
        }
        catch (ArgumentException ex) { TempData["Error"] = ex.Message; }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        catch (HttpRequestException ex) { TempData["Error"] = ex.Message; }
        catch (InvalidDataException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index), new { PeriodoEscolarId = periodoEscolarId });
    }

    private int? GetEntidadAcademicaAutorizadaId()
    {
        if (!User.IsInRole(Constantes.COORDINADOR_EA)) return null;
        return int.TryParse(User.FindFirstValue("EntidadAcademicaId"), out var id) && id > 0 ? id : -1;
    }

    private async Task<ProgramacionAcademicaImportarViewModel> PrepararImportacionAsync(
        ProgramacionAcademicaImportarViewModel modelo, CancellationToken cancellationToken)
    {
        var catalogos = await _catalogos.ObtenerAsync(cancellationToken);
        modelo.Regiones = catalogos.Regiones;
        modelo.Entidades = await _catalogos.ObtenerEntidadesAsync(null, null, cancellationToken);
        modelo.Periodos = await _service.ObtenerPeriodosAsync(cancellationToken);
        return modelo;
    }
}
