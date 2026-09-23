using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers;

[Authorize(Policy = PoliticasAutorizacion.SuperUsuario)]
public sealed class CatalogosController : Controller
{
    private readonly ICatalogosMvcService _catalogos;

    public CatalogosController(ICatalogosMvcService catalogos) => _catalogos = catalogos;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await _catalogos.ObtenerAsync(cancellationToken));

    [HttpGet]
    public Task<IActionResult> Campus(int? regionId, CancellationToken cancellationToken) =>
        Opciones(_catalogos.ObtenerCampusAsync(regionId, cancellationToken));

    [HttpGet]
    public Task<IActionResult> Entidades(int? campusId, int? areaAcademicaId, CancellationToken cancellationToken) =>
        Opciones(_catalogos.ObtenerEntidadesAsync(campusId, areaAcademicaId, cancellationToken));

    [HttpGet]
    public Task<IActionResult> Programas(int? entidadAcademicaId, CancellationToken cancellationToken) =>
        Opciones(_catalogos.ObtenerProgramasAsync(entidadAcademicaId, cancellationToken));

    [HttpGet]
    public Task<IActionResult> Planes(int? programaEducativoId, CancellationToken cancellationToken) =>
        Opciones(_catalogos.ObtenerPlanesAsync(programaEducativoId, cancellationToken));

    [HttpGet]
    public Task<IActionResult> Experiencias(int? planEstudiosId, CancellationToken cancellationToken) =>
        Opciones(_catalogos.ObtenerExperienciasAsync(planEstudiosId, cancellationToken));

    private static async Task<IActionResult> Opciones(Task<IReadOnlyList<Models.ViewModels.Catalogos.CatalogoOpcion>> task) =>
        new JsonResult(await task);
}
