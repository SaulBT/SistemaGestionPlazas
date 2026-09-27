using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models.DTOs.Catalogos;
using SGPla.Models.ViewModels.Catalogos;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers;

[Authorize(Policy = PoliticasAutorizacion.SuperUsuario)]
public sealed class CatalogosController : Controller
{
    private readonly ICatalogosMvcService _catalogos;
    private readonly IAdministracionCatalogosMvcService _administracion;

    public CatalogosController(ICatalogosMvcService catalogos, IAdministracionCatalogosMvcService administracion)
    {
        _catalogos = catalogos;
        _administracion = administracion;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(new CatalogosAdministracionIndexViewModel
        {
            Catalogos = await _catalogos.ObtenerAsync(cancellationToken),
            Administrables = await _administracion.ListarAsync(cancellationToken)
        });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearClasificacion(CrearCatalogoClasificacionMvcDto datos, CancellationToken cancellationToken)
    {
        try
        {
            await _administracion.CrearClasificacionAsync(datos, cancellationToken);
            TempData["Success"] = "La clasificación se creó correctamente.";
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarNombre(EditarNombreCatalogoMvcDto datos, CancellationToken cancellationToken)
    {
        try
        {
            await _administracion.EditarNombreAsync(datos, cancellationToken);
            TempData["Success"] = "El catálogo se actualizó correctamente.";
        }
        catch (Exception exception) when (exception is ArgumentException or KeyNotFoundException or InvalidOperationException or Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarActivo(TipoCatalogoNormalizado tipo, int id, bool activo,
        CancellationToken cancellationToken)
    {
        try
        {
            await _administracion.CambiarActivoAsync(tipo, id, activo, cancellationToken);
            TempData["Success"] = activo ? "La clasificación se reactivó." : "La clasificación se dio de baja.";
        }
        catch (Exception exception) when (exception is ArgumentException or KeyNotFoundException or InvalidOperationException)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public Task<IActionResult> Campus(int? regionId, CancellationToken cancellationToken) =>
        Opciones(_catalogos.ObtenerCampusAsync(regionId, cancellationToken));

    [HttpGet]
    public Task<IActionResult> Entidades(int? campusId, int? areaAcademicaId, int? regionId, CancellationToken cancellationToken) =>
        Opciones(_catalogos.ObtenerEntidadesAsync(campusId, areaAcademicaId, cancellationToken, regionId));

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
