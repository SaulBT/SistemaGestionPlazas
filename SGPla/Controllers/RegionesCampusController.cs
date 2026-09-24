using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models.DTOs.Catalogos;
using SGPla.Models.ViewModels.Catalogos;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers;

[Authorize(Policy = PoliticasAutorizacion.SuperUsuario)]
public sealed class RegionesCampusController : Controller
{
    private readonly IRegionCampusMvcService _service;

    public RegionesCampusController(IRegionCampusMvcService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await CargarAsync(cancellationToken));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearRegion(CrearRegionViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) TempData["Error"] = "Revisa la clave y el nombre de la región.";
        else
        {
            try
            {
                await _service.CrearRegionAsync(new CrearRegionMvcDto { Clave = modelo.Clave, Nombre = modelo.Nombre }, cancellationToken);
                TempData["Success"] = "La región se creó correctamente.";
            }
            catch (ArgumentException ex) { TempData["Error"] = ex.Message; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarNombreRegion(EditarNombreCatalogoViewModel modelo, CancellationToken cancellationToken) =>
        await EditarNombreAsync(modelo, _service.EditarNombreRegionAsync, "La región", cancellationToken);

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarRegion(int id, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _service.DarDeBajaRegionAsync(id, cancellationToken)) return NotFound();
            TempData["Success"] = "La región se dio de baja.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCampus(CrearCampusViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) TempData["Error"] = "Revisa la clave, el nombre y la región seleccionada.";
        else
        {
            try
            {
                await _service.CrearCampusAsync(new CrearCampusMvcDto { Clave = modelo.Clave, Nombre = modelo.Nombre, RegionId = modelo.RegionId }, cancellationToken);
                TempData["Success"] = "El Campus se creó correctamente.";
            }
            catch (ArgumentException ex) { TempData["Error"] = ex.Message; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarNombreCampus(EditarNombreCatalogoViewModel modelo, CancellationToken cancellationToken) =>
        await EditarNombreAsync(modelo, _service.EditarNombreCampusAsync, "El Campus", cancellationToken);

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarCampus(int id, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _service.DarDeBajaCampusAsync(id, cancellationToken)) return NotFound();
            TempData["Success"] = "El Campus se dio de baja.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> EditarNombreAsync(
        EditarNombreCatalogoViewModel modelo,
        Func<int, string, CancellationToken, Task<bool>> editar,
        string entidad,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) TempData["Error"] = "El nombre es obligatorio y debe tener hasta 200 caracteres.";
        else if (!await editar(modelo.Id, modelo.Nombre, cancellationToken)) return NotFound();
        else TempData["Success"] = $"{entidad} actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<RegionCampusAdminViewModel> CargarAsync(CancellationToken cancellationToken)
    {
        var resultado = await _service.ObtenerAsync(cancellationToken);
        return new RegionCampusAdminViewModel { Regiones = resultado.Regiones, Campus = resultado.Campus };
    }
}
