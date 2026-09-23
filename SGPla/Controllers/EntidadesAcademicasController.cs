using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGPla.Commons;
using SGPla.Commons.Factories;
using SGPla.Models.Components;
using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Models.ViewModels.Catalogos;
using SGPla.Models.ViewModels.EntidadesAcademicas;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers;

[Authorize(Policy = PoliticasAutorizacion.SuperUsuario)]
public sealed class EntidadesAcademicasController : Controller
{
    private static readonly List<string> Headers = ["Clave", "Nombre", "Domicilio", "Campus", "Municipio", "Área Académica", "Región", "Acciones"];
    private readonly IEntidadAcademicaMvcService _entidades;
    private readonly ICatalogosMvcService _catalogos;
    private readonly ILogger<EntidadesAcademicasController> _logger;

    public EntidadesAcademicasController(
        IEntidadAcademicaMvcService entidades,
        ICatalogosMvcService catalogos,
        ILogger<EntidadesAcademicasController> logger)
    {
        _entidades = entidades;
        _catalogos = catalogos;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? busqueda, int? regionId, int? areaAcademicaId,
        int pagina = 1, int cantidad = 10, CancellationToken cancellationToken = default)
    {
        var currentPage = Math.Max(1, pagina);
        var pageSize = Math.Clamp(cantidad, 1, 100);
        var catalogos = await _catalogos.ObtenerCatalogosEntidadAcademicaAsync(regionId, cancellationToken);
        var resultado = await _entidades.BuscarAsync(
            new FiltroEntidadAcademicaMvcDto(regionId, areaAcademicaId, busqueda,
                currentPage, pageSize), cancellationToken);
        var rows = resultado.Items.Select(entidad => new TableRowModel
        {
            Cells =
            [
                new() { Value = entidad.Clave },
                new() { Value = entidad.Nombre },
                new() { Value = Domicilio(entidad) },
                new() { Value = entidad.CampusNombre },
                new() { Value = entidad.MunicipioNombre },
                new() { Value = entidad.AreaAcademicaNombre },
                new() { Value = entidad.RegionNombre },
                new()
                {
                    Actions =
                    [
                        new() { Accion = "ver", Url = Url.Action(nameof(VerEntidadAcademica), new { id = entidad.Id }) },
                        new() { Accion = "programa educativo", Url = Url.Action("Buscar", "ProgramasEducativos", new { idEntidadAcademica = entidad.Id, region = entidad.RegionNombre, idAreaAcademica = entidad.AreaAcademicaId }) },
                        new() { Accion = "editar", Url = Url.Action(nameof(EditarEntidadAcademica), new { id = entidad.Id }) },
                        new() { Accion = "eliminar", OnClick = $"abrirModalConfirmacion('¿Desea eliminar esta entidad académica y sus datos académicos dependientes?', function() {{ eliminarEntidadAcademica({entidad.Id}); }})" }
                    ]
                }
            ]
        }).ToList();

        var table = resultado.TotalCount == 0
            ? TablaFactory.GenerarTablaConMensaje(Headers, string.Format(Constantes.TABLA_VACIA, Constantes.ENTIDADES_ACADEMICAS))
            : new TableModel
            {
                Headers = Headers,
                Rows = rows,
                Pagination = new PaginationInfo
                {
                    CurrentPage = currentPage,
                    PageSize = pageSize,
                    TotalItems = resultado.TotalCount,
                    OnPageChange = "cambiarPagina"
                }
            };

        return View(new IndexViewModel
        {
            Table = table,
            Regiones = Options(catalogos.Regiones, regionId),
            Areas = Options(catalogos.AreasAcademicas, areaAcademicaId),
            RegionIdSeleccionada = regionId,
            AreaAcademicaIdSeleccionada = areaAcademicaId,
            Busqueda = busqueda,
            PaginaActual = currentPage,
            CantidadPorPagina = pageSize
        });
    }

    [HttpGet]
    public async Task<IActionResult> CrearEntidadAcademica(int? regionId, int? campusId, int? areaAcademicaId,
        CancellationToken cancellationToken)
    {
        var model = new CrearEntidadAcademicaViewModel
        {
            RegionId = regionId,
            CampusId = campusId,
            AreaAcademicaId = areaAcademicaId
        };
        await CargarCombosAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearEntidadAcademica(CrearEntidadAcademicaViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _entidades.CrearAsync(ToInput(model), cancellationToken);
                TempData["Success"] = "Entidad académica creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error al crear una entidad académica");
                ModelState.AddModelError(string.Empty, "No fue posible guardar la entidad académica.");
            }
        }

        await CargarCombosAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> VerEntidadAcademica(int id, CancellationToken cancellationToken)
    {
        var entidad = await _entidades.ObtenerPorIdAsync(id, cancellationToken);
        return entidad is null ? NotFound() : View(entidad);
    }

    [HttpGet]
    public async Task<IActionResult> EditarEntidadAcademica(int id, CancellationToken cancellationToken)
    {
        var entidad = await _entidades.ObtenerPorIdAsync(id, cancellationToken);
        if (entidad is null) return NotFound();

        var model = new CrearEntidadAcademicaViewModel
        {
            IdEntidadAcademica = entidad.Id,
            Clave = entidad.Clave,
            Nombre = entidad.Nombre,
            Calle = entidad.Calle,
            NumeroExterior = entidad.NumeroExterior,
            Colonia = entidad.Colonia,
            CodigoPostal = entidad.CodigoPostal,
            Telefono = entidad.Telefono,
            Extension = entidad.Extension,
            CampusId = entidad.CampusId,
            AreaAcademicaId = entidad.AreaAcademicaId,
            MunicipioId = entidad.MunicipioId,
            RegionId = entidad.RegionId
        };
        await CargarCombosAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarEntidadAcademica(CrearEntidadAcademicaViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _entidades.ActualizarAsync(model.IdEntidadAcademica, ToInput(model), cancellationToken);
                TempData["Success"] = "Entidad académica actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or KeyNotFoundException)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error al actualizar entidad académica {Id}", model.IdEntidadAcademica);
                ModelState.AddModelError(string.Empty, "No fue posible actualizar la entidad académica.");
            }
        }

        await CargarCombosAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarEntidadAcademica(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _entidades.EliminarAsync(id, cancellationToken);
            TempData["Success"] = "La entidad académica y sus datos académicos dependientes se dieron de baja.";
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or KeyNotFoundException)
        {
            TempData["Error"] = exception.Message;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error al eliminar entidad académica {Id}", id);
            TempData["Error"] = "No fue posible dar de baja la entidad académica.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCombosAsync(CrearEntidadAcademicaViewModel model, CancellationToken cancellationToken)
    {
        var catalogos = await _catalogos.ObtenerCatalogosEntidadAcademicaAsync(model.RegionId, cancellationToken);
        model.Regiones = Options(catalogos.Regiones, model.RegionId);
        model.Campus = Options(await _catalogos.ObtenerCampusAsync(model.RegionId, cancellationToken), model.CampusId);
        model.Areas = Options(catalogos.AreasAcademicas, model.AreaAcademicaId);
        model.Municipios = Options(catalogos.Municipios, model.MunicipioId);
    }

    private static List<OptionModel> Options(IEnumerable<CatalogoOpcion> source, int? selectedId) =>
        source.Select(x => new OptionModel
        {
            Value = x.Id.ToString(),
            Text = $"{x.Clave} - {x.Nombre}",
            Selected = selectedId == x.Id
        }).ToList();

    private static EntidadAcademicaMvcInputDto ToInput(CrearEntidadAcademicaViewModel model) => new()
    {
        Clave = model.Clave,
        Nombre = model.Nombre,
        Calle = model.Calle,
        NumeroExterior = model.NumeroExterior,
        Colonia = model.Colonia,
        CodigoPostal = model.CodigoPostal,
        Telefono = model.Telefono,
        Extension = model.Extension,
        CampusId = model.CampusId ?? 0,
        AreaAcademicaId = model.AreaAcademicaId ?? 0,
        MunicipioId = model.MunicipioId ?? 0
    };

    private static string Domicilio(EntidadAcademicaMvcDto entidad) =>
        $"{entidad.Calle}{(string.IsNullOrWhiteSpace(entidad.NumeroExterior) ? string.Empty : $" {entidad.NumeroExterior}")}, {entidad.Colonia}, C.P. {entidad.CodigoPostal}";
}
