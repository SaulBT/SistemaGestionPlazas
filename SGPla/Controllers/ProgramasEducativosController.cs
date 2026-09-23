using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models.Components;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Models.ViewModels.Catalogos;
using SGPla.Models.ViewModels.ProgramasEducativos;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers;

[Authorize(Policy = PoliticasAutorizacion.SuperUsuario)]
public sealed class ProgramasEducativosController : Controller
{
    private readonly IProgramaEducativoMvcService _service;
    private readonly ICatalogosMvcService _catalogos;
    private readonly ILogger<ProgramasEducativosController> _logger;

    public ProgramasEducativosController(IProgramaEducativoMvcService service, ICatalogosMvcService catalogos,
        ILogger<ProgramasEducativosController> logger)
    { _service = service; _catalogos = catalogos; _logger = logger; }

    [HttpGet]
    public async Task<IActionResult> Index(string? busqueda, int? regionId, int? idAreaAcademica,
        int? idEntidadAcademica, int pagina = 1, int cantidad = 10, CancellationToken cancellationToken = default)
    {
        var catalogos = await _catalogos.ObtenerAsync(cancellationToken);
        var entidades = await _service.ObtenerEntidadesAsync(regionId, idAreaAcademica, cancellationToken);
        var resultado = await _service.BuscarAsync(new ProgramaEducativoMvcFiltro
        {
            Nombre = busqueda, RegionId = regionId, AreaAcademicaId = idAreaAcademica,
            EntidadAcademicaId = idEntidadAcademica, Pagina = pagina, TamanoPagina = cantidad
        }, cancellationToken);
        var totalPaginas = (int)Math.Ceiling(resultado.Total / (double)Math.Clamp(cantidad, 1, 100));
        if (totalPaginas > 0 && pagina > totalPaginas)
        {
            pagina = totalPaginas;
            resultado = await _service.BuscarAsync(new ProgramaEducativoMvcFiltro
            { Nombre = busqueda, RegionId = regionId, AreaAcademicaId = idAreaAcademica,
              EntidadAcademicaId = idEntidadAcademica, Pagina = pagina, TamanoPagina = cantidad }, cancellationToken);
        }
        var options = resultado.Items.Select(x => new TableRowModel
        {
            Cells =
            [
                new() { Value = x.Nombre }, new() { Value = x.Region }, new() { Value = x.AreaAcademica },
                new() { Value = x.EntidadAcademica }, new() { Value = x.SistemaEducativo },
                new() { Value = x.NivelFormacion },
                new() { Actions =
                    [
                        new() { Accion = "ver", Url = Url.Action(nameof(VerProgramaEducativo), new { id = x.Id }) },
                        new() { Accion = "editar", Url = Url.Action(nameof(EditarProgramaEducativo), new { id = x.Id }) },
                        new() { Accion = "eliminar", OnClick = $"abrirModalConfirmacion('¿Desea eliminar este programa educativo?', function() {{ eliminarProgramaEducativo({x.Id}); }})" }
                    ] }
            ]
        }).ToList();
        var table = resultado.Total == 0
            ? new TableModel { Headers = ["Nombre", "Región", "Área Académica", "Entidad Académica", "Sistema Educativo", "Nivel", "Acciones"], Rows = [] }
            : new TableModel
            {
                Headers = ["Nombre", "Región", "Área Académica", "Entidad Académica", "Sistema Educativo", "Nivel", "Acciones"],
                Rows = options,
                Pagination = new PaginationInfo { CurrentPage = pagina, PageSize = cantidad, TotalItems = resultado.Total, OnPageChange = "cambiarPagina" }
            };
        return View(new IndexViewModel
        {
            Table = table,
            Regiones = Map(catalogos.Regiones, regionId),
            Areas = Map(catalogos.AreasAcademicas, idAreaAcademica),
            Entidades = Map(entidades, idEntidadAcademica),
            RegionSeleccionadaId = regionId,
            IdAreaSeleccionada = idAreaAcademica,
            IdEntidadSeleccionada = idEntidadAcademica,
            Busqueda = busqueda,
            PaginaActual = pagina,
            CantidadPorPagina = cantidad
        });
    }

    [HttpGet]
    public async Task<IActionResult> CrearProgramaEducativo(CancellationToken cancellationToken = default) =>
        View(await CrearModeloAsync(new CrearProgramaEducativoViewModel(), cancellationToken));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearProgramaEducativo(CrearProgramaEducativoViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return View(await CrearModeloAsync(model, cancellationToken));
        try
        {
            await _service.GuardarAsync(ToDto(model), cancellationToken);
            TempData["Success"] = "Programa educativo creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await CrearModeloAsync(model, cancellationToken));
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditarProgramaEducativo(int id, CancellationToken cancellationToken = default)
    {
        var programa = await _service.ObtenerAsync(id, cancellationToken);
        if (programa is null) return NotFound();
        var model = new CrearProgramaEducativoViewModel
        {
            IdProgramaEducativo = id, Nombre = programa.Nombre, IdEntidadAcademica = programa.EntidadAcademicaId,
            IdSistemaEducativo = programa.SistemaEducativoId, IdNivelFormacion = programa.NivelFormacionId,
            RegionId = programa.RegionId, IdAreaAcademica = programa.AreaAcademicaId
        };
        return View(await CrearModeloAsync(model, cancellationToken));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarProgramaEducativo(CrearProgramaEducativoViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return View(await CrearModeloAsync(model, cancellationToken));
        try
        {
            await _service.GuardarAsync(ToDto(model, model.IdProgramaEducativo), cancellationToken);
            TempData["Success"] = "Programa educativo actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or KeyNotFoundException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await CrearModeloAsync(model, cancellationToken));
        }
    }

    [HttpGet]
    public async Task<IActionResult> VerProgramaEducativo(int id, CancellationToken cancellationToken = default)
    {
        var dto = await _service.ObtenerAsync(id, cancellationToken);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarProgramaEducativo(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await _service.EliminarAsync(id, cancellationToken)) TempData["Error"] = "El programa educativo no existe o ya está inactivo.";
            else TempData["Success"] = "Programa educativo eliminado correctamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar programa educativo {ProgramaId}", id);
            TempData["Error"] = "No fue posible eliminar el programa educativo.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerEntidades(int? regionId, int? idArea, CancellationToken cancellationToken = default) =>
        Json(await _service.ObtenerEntidadesAsync(regionId, idArea, cancellationToken));

    private async Task<CrearProgramaEducativoViewModel> CrearModeloAsync(CrearProgramaEducativoViewModel model, CancellationToken cancellationToken)
    {
        var catalogos = await _catalogos.ObtenerAsync(cancellationToken);
        var entidades = await _service.ObtenerEntidadesAsync(model.RegionId, model.IdAreaAcademica, cancellationToken);
        model.Regiones = Map(catalogos.Regiones, model.RegionId);
        model.Areas = Map(catalogos.AreasAcademicas, model.IdAreaAcademica);
        model.Entidades = Map(entidades, model.IdEntidadAcademica);
        model.SistemasEducativos = Map(catalogos.SistemasEducativos, model.IdSistemaEducativo);
        model.NivelesFormacion = Map(catalogos.NivelesFormacion, model.IdNivelFormacion);
        return model;
    }

    private static GuardarProgramaEducativoMvcDto ToDto(CrearProgramaEducativoViewModel model, int? id = null) => new()
    {
        Id = id, Nombre = model.Nombre, EntidadAcademicaId = model.IdEntidadAcademica!.Value,
        SistemaEducativoId = model.IdSistemaEducativo!.Value, NivelFormacionId = model.IdNivelFormacion!.Value
    };

    private static List<OptionModel> Map(IEnumerable<CatalogoOpcion> opciones, int? seleccionado) => opciones
        .Select(x => new OptionModel { Value = x.Id.ToString(), Text = x.Nombre, Selected = seleccionado == x.Id }).ToList();
}
