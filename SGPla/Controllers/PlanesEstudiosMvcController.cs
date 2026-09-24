using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Models.ViewModels.PlanesEstudios;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers;

[Authorize(Policy = PoliticasAutorizacion.SuperUsuario)]
public sealed class PlanesEstudiosController : Controller
{
    private readonly IPlanEstudiosMvcService _planes;
    private readonly ICatalogosMvcService _catalogos;
    private readonly IPlanEstudiosExcelImportador _importador;

    public PlanesEstudiosController(IPlanEstudiosMvcService planes, ICatalogosMvcService catalogos, IPlanEstudiosExcelImportador importador)
    {
        _planes = planes;
        _catalogos = catalogos;
        _importador = importador;
    }

    [HttpGet]
    public async Task<IActionResult> Index(PlanEstudiosMvcFiltro filtro, CancellationToken cancellationToken)
    {
        filtro = filtro with { Pagina = Math.Max(1, filtro.Pagina), TamanoPagina = Math.Clamp(filtro.TamanoPagina, 1, 100) };
        var pagina = await _planes.BuscarAsync(filtro, cancellationToken);
        var catalogos = await _catalogos.ObtenerAsync(cancellationToken);
        var entidades = await _catalogos.ObtenerEntidadesAsync(null, filtro.AreaAcademicaId, cancellationToken, filtro.RegionId);
        var programas = await _catalogos.ObtenerProgramasAsync(filtro.EntidadAcademicaId, cancellationToken);
        return View(new PlanEstudiosMvcIndexViewModel
        {
            Filtro = filtro,
            Items = pagina.Items,
            Total = pagina.Total,
            Regiones = catalogos.Regiones,
            Areas = catalogos.AreasAcademicas,
            Entidades = entidades,
            Programas = programas
        });
    }

    [HttpGet]
    public async Task<IActionResult> Crear(CancellationToken cancellationToken) =>
        View(new PlanEstudiosMvcCrearViewModel { Programas = await _catalogos.ObtenerProgramasAsync(null, cancellationToken) });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(PlanEstudiosMvcCrearViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            modelo.Programas = await _catalogos.ObtenerProgramasAsync(null, cancellationToken);
            return View(modelo);
        }
        try
        {
            var id = await _planes.CrearAsync(new GuardarPlanEstudiosMvcDto
            {
                ProgramaEducativoId = modelo.ProgramaEducativoId,
                Codigo = modelo.Codigo
            }, cancellationToken);
            TempData["Success"] = "El Plan de Estudios se creó correctamente.";
            return RedirectToAction(nameof(Ver), new { id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        modelo.Programas = await _catalogos.ObtenerProgramasAsync(null, cancellationToken);
        return View(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Ver(int id, string? busqueda, int pagina = 1, int tamanoPagina = 20, CancellationToken cancellationToken = default)
    {
        var plan = await _planes.ObtenerAsync(id, cancellationToken);
        if (plan is null) return NotFound();
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 100);
        var experiencias = await _planes.BuscarExperienciasAsync(id, busqueda, pagina, tamanoPagina, cancellationToken);
        return View(new PlanEstudiosMvcDetalleViewModel
        {
            Plan = plan,
            Experiencias = experiencias.Items,
            TotalExperiencias = experiencias.Total,
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            Busqueda = busqueda,
            AreasFormacion = (await _catalogos.ObtenerAsync(cancellationToken)).AreasFormacion
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> ImportarExcel(int planId, int areaFormacionId, IFormFile? archivo, CancellationToken cancellationToken)
    {
        var plan = await _planes.ObtenerAsync(planId, cancellationToken);
        if (plan is null) return NotFound();
        if (archivo is null || archivo.Length == 0 || archivo.Length > 10 * 1024 * 1024 ||
            !(Path.GetExtension(archivo.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
              Path.GetExtension(archivo.FileName).Equals(".xls", StringComparison.OrdinalIgnoreCase)))
        {
            TempData["Error"] = "Selecciona un archivo Excel válido de hasta 10 MB.";
            return RedirectToAction(nameof(Ver), new { id = planId });
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var stream = archivo.OpenReadStream();
            var importacion = _importador.Leer(stream, plan.Codigo, cancellationToken);
            if (importacion.Error is not null)
            {
                TempData["Error"] = importacion.Error;
                return RedirectToAction(nameof(Ver), new { id = planId });
            }
            var cantidad = await _planes.ImportarExperienciasAsync(planId, areaFormacionId, importacion.Experiencias, cancellationToken);
            TempData["Success"] = $"Se importaron {cantidad} Experiencias Educativas. El archivo no se almacenó.";
        }
        catch (ArgumentException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Ver), new { id = planId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        if (!await _planes.EliminarAsync(id, cancellationToken)) return NotFound();
        TempData["Success"] = "El Plan de Estudios y sus Experiencias Educativas vigentes se dieron de baja.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> CrearExperiencia(int planId, CancellationToken cancellationToken)
    {
        if (await _planes.ObtenerAsync(planId, cancellationToken) is null) return NotFound();
        return View(await PrepararExperienciaAsync(new PlanEstudiosMvcExperienciaViewModel { PlanEstudiosId = planId }, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearExperiencia(PlanEstudiosMvcExperienciaViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(await PrepararExperienciaAsync(modelo, cancellationToken));
        try
        {
            await _planes.CrearExperienciaAsync(Convertir(modelo), cancellationToken);
            TempData["Success"] = "La Experiencia Educativa se agregó al Plan.";
            return RedirectToAction(nameof(Ver), new { id = modelo.PlanEstudiosId });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        return View(await PrepararExperienciaAsync(modelo, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> EditarExperiencia(int planId, int id, CancellationToken cancellationToken)
    {
        var experiencia = await _planes.ObtenerExperienciaAsync(planId, id, cancellationToken);
        if (experiencia is null) return NotFound();
        return View(await PrepararExperienciaAsync(new PlanEstudiosMvcExperienciaViewModel
        {
            Id = experiencia.Id,
            PlanEstudiosId = planId,
            Nombre = experiencia.Nombre,
            MateriaEe = experiencia.MateriaEe,
            CursoEe = experiencia.CursoEe,
            HorasTeoricas = experiencia.HorasTeoricas,
            HorasPracticas = experiencia.HorasPracticas,
            Creditos = experiencia.Creditos,
            PerfilDocente = experiencia.PerfilDocente,
            AreaFormacionId = experiencia.AreaFormacionId,
            DatosAcademicosBloqueados = await _planes.TieneProgramacionAsync(id, cancellationToken)
        }, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarExperiencia(PlanEstudiosMvcExperienciaViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(await PrepararExperienciaAsync(modelo, cancellationToken));
        try
        {
            if (!await _planes.ActualizarExperienciaAsync(Convertir(modelo), cancellationToken)) return NotFound();
            TempData["Success"] = "La Experiencia Educativa se actualizó correctamente.";
            return RedirectToAction(nameof(Ver), new { id = modelo.PlanEstudiosId });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        return View(await PrepararExperienciaAsync(modelo, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarExperiencia(int planId, int id, CancellationToken cancellationToken)
    {
        if (!await _planes.EliminarExperienciaAsync(planId, id, cancellationToken)) return NotFound();
        TempData["Success"] = "La Experiencia Educativa se dio de baja.";
        return RedirectToAction(nameof(Ver), new { id = planId });
    }

    private async Task<PlanEstudiosMvcExperienciaViewModel> PrepararExperienciaAsync(
        PlanEstudiosMvcExperienciaViewModel modelo, CancellationToken cancellationToken)
    {
        var catalogs = await _catalogos.ObtenerAsync(cancellationToken);
        modelo.AreasFormacion = catalogs.AreasFormacion;
        return modelo;
    }

    private static GuardarExperienciaEducativaMvcDto Convertir(PlanEstudiosMvcExperienciaViewModel modelo) => new()
    {
        Id = modelo.Id,
        PlanEstudiosId = modelo.PlanEstudiosId,
        Nombre = modelo.Nombre,
        MateriaEe = modelo.MateriaEe,
        CursoEe = modelo.CursoEe,
        HorasTeoricas = modelo.HorasTeoricas,
        HorasPracticas = modelo.HorasPracticas,
        Creditos = modelo.Creditos,
        PerfilDocente = modelo.PerfilDocente,
        AreaFormacionId = modelo.AreaFormacionId
    };
}
