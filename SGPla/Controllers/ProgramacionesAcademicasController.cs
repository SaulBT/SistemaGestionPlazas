using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.ViewModels.ProgramacionesAcademicas;
using SGPla.Services.Interfaces;
using System.Text.Json;

namespace SGPla.Controllers;

public class ProgramacionesAcademicasController : Controller
{
    private readonly ILogger<ProgramacionesAcademicasController> _logger;
    private readonly IProgramacionAcademicaService _programacionAcademicaService;
    private readonly IPeriodoEscolarService _periodoEscolarService;
    private int _paginaActual = 1;
    private static List<string> HEADERS_TABLA_INDEX = ["Programa educativo", "Periodo", "No. EE Asignadas", "No. EE Vacantes", "Acciones"];


    public ProgramacionesAcademicasController(
        ILogger<ProgramacionesAcademicasController> logger,
        IProgramacionAcademicaService programacionAcademicaService,
        IPeriodoEscolarService periodoEscolarService)
    {
        _logger = logger;
        _programacionAcademicaService = programacionAcademicaService;
        _periodoEscolarService = periodoEscolarService;
    }

    [HttpGet]
    public async Task<IActionResult> CargarProgramacionAcademicaPaso1(string? busqueda, string? region, int? idEntidadAcademica, int? idPeriodo, int pagina = 1, int cantidad = 10)
    {
        //HttpContext.Session.Remove("Ofertas");
        //HttpContext.Session.Remove("Cargas");

        _paginaActual = pagina;

        var regionesCombo = Constantes.REGIONES
               .Select(r => new OptionModel { Value = r, Text = r, Selected = r == region })
               .ToList();

        var periodos = await _periodoEscolarService.ObtenerTodosAsync();

        var periodosCombo = periodos
            .Select(p => new OptionModel
            {
                Value = p.IdPeriodoEscolar.ToString(),
                Text = p.PeriodoMostrar,
                Selected = idPeriodo.HasValue && p.IdPeriodoEscolar == idPeriodo.Value
            })
            .ToList();

        List<EntidadAcademica> entidades;

        entidades = await _programacionAcademicaService.ObtenerOpcionesEntidadAcademicaAsync(region);


        var entidadesCombo = entidades
            .Select(e => new OptionModel
            {
                Value = e.IdEntidadAcademica.ToString(),
                Text = e.Nombre,
                Selected = idEntidadAcademica.HasValue && e.IdEntidadAcademica == idEntidadAcademica.Value
            })
            .ToList();

        return View(new CargarProgramacionAcademica1ViewModel
        {
            Table = null,//await LlenarTabla(busqueda, region, idEntidadAcademica, pagina, cantidad),
            Regiones = regionesCombo,
            Entidades = entidadesCombo,
            RegionSeleccionada = region,
            IdEntidadSeleccionada = idEntidadAcademica,
            Busqueda = busqueda,
            PaginaActual = _paginaActual,
            CantidadPorPagina = cantidad,
            Periodos = periodosCombo,
            IdPeriodoSeleccionado = idEntidadAcademica
        });
    }



    //private async Task<TableModel> LlenarTabla(string? busqueda, string? region, int? idEntidadAcademica, int pagina = 1, int cantidad = 10)
    //{
    //    try
    //    {
    //        var filtros = new BuscarProgramaEducativoDTO
    //        {
    //            Nombre = busqueda,
    //            Region = region,
    //            IdEntidadAcademica = idEntidadAcademica,
    //            Pagina = pagina,
    //            Cantidad = cantidad
    //        };

    //        //var resultado = await _programaEducativoService.BuscarPorFiltroPaginadoAsync(filtros);
    //        //if (resultado.Items.Count == 0)
    //        //{
    //        //    _paginaActual = 1;
    //        //    filtros.Pagina = _paginaActual;
    //        //    resultado = await _programaEducativoService.BuscarPorFiltroPaginadoAsync(filtros);
    //        //}
    //        //if (resultado.Items.Count == 0)
    //        //    return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_INDEX, string.Format(Constantes.TABLA_VACIA, Constantes.PROGRAMAS_EDUCATIVOS));


    //        //if (resultado.Items == null || !resultado.Items.Any())
    //        //{
    //        //    return new TableModel
    //        //    {
    //        //        Headers = HEADERS_TABLA_INDEX,
    //        //        Rows = new List<TableRowModel>(),
    //        //        Pagination = new PaginationInfo
    //        //        {
    //        //            CurrentPage = _paginaActual,
    //        //            PageSize = cantidad,
    //        //            TotalItems = 0,
    //        //            OnPageChange = "cambiarPagina"
    //        //        }
    //        //    };
    //        //}

    //        return new TableModel
    //        {
    //            Headers = new List<string> { "Nombre", "Región", "Área Académica", "Entidad Académica", "Acciones" },
    //            Rows = resultado.Items.Select(programa => new TableRowModel
    //            {
    //                Cells = new List<TableCellModel>
    //                    {
    //                        new() { Value = programa.Nombre },
    //                        new() { Value = programa.Region },
    //                        new() { Value = programa.AreaAcademica },
    //                        new() { Value = programa.EntidadAcademica },
    //                        new()
    //                        {
    //                            Actions = new List<TableActionModel>
    //                            {
    //                                new()
    //                                {
    //                                    Accion = "ver",
    //                                    Url = Url.Action("VerProgramaEducativo", "ProgramasEducativos", new { id = programa.IdProgramaEducativo })
    //                                },
    //                                new()
    //                                {
    //                                     Accion = "plan de estudios",
    //                                    Url = Url.Action("Index", "PlanesEstudios", new
    //                                    {
    //                                        region = programa.Region,
    //                                        idEntidadAcademica = programa.IdEntidadAcademica,
    //                                        idAreaAcademica=programa.IdAreaAcademica,
    //                                        idProgramaEducativo = programa.IdProgramaEducativo

    //                                    })
    //                                },
    //                                new()
    //                                {
    //                                    Accion = "editar",
    //                                    Url = Url.Action("EditarProgramaEducativo", "ProgramasEducativos", new { id = programa.IdProgramaEducativo })
    //                                },
    //                                new()
    //                                {
    //                                    Accion = "eliminar",
    //                                    OnClick = $"abrirModalConfirmacion('¿Desea eliminar este programa educativo?', function() {{ eliminarProgramaEducativo({programa.IdProgramaEducativo}); }})"
    //                                }
    //                            }
    //                        }
    //                    }
    //            }).ToList(),
    //            Pagination = new PaginationInfo
    //            {
    //                CurrentPage = _paginaActual,
    //                PageSize = cantidad,
    //                TotalItems = resultado.TotalCount,
    //                OnPageChange = "cambiarPagina"
    //            }
    //        };
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error al obtener la lista de programas educativos");
    //        TempData["Error"] = "Error al cargar los programas educativos";
    //        return new TableModel();
    //    }
    //}

    [HttpPost]
    public async Task<IActionResult> CargarVacantes(IFormFile archivoVacantes)
    {
        return await CargarOfertas(archivoVacantes);
    }

    [HttpPost]
    public async Task<IActionResult> CargarDescargas(IFormFile archivoDescargas)
    {
        return await CargarOfertas(archivoDescargas);
    }

    [HttpPost]
    public async Task<IActionResult> CargarOfertas(IFormFile archivo)
    {
        var vm = ObtenerViewModelDesdeSesion();

        if (archivo is null || archivo.Length == 0)
        {
            vm.Error = "Selecciona un archivo antes de continuar.";
            return View("CargarProgramacionAcademicaPaso1", vm);
        }

        try
        {
            var ofertasExistentes = ObtenerOfertasSesion();

            var nuevasOfertas =
                await _programacionAcademicaService
                    .ProcesarArchivoAsync(archivo);

            ofertasExistentes.AddRange(nuevasOfertas);

            HttpContext.Session.SetString(
                "Ofertas",
                JsonSerializer.Serialize(ofertasExistentes));

            vm.OfertasVacantes = ofertasExistentes
                .Where(o => o.NP == null)
                .ToList();

            vm.OfertasAsignadas = ofertasExistentes
                .Where(o => o.NP != null)
                .ToList();
        }
        catch (Exception ex)
        {
            vm.Error = $"Error al procesar el archivo: {ex.Message}";
        }

        return View("CargarProgramacionAcademicaPaso1", vm);
    }


    [HttpPost]
    public async Task<IActionResult> CargarCargas(IFormFile archivoCarga)
    {
        var vm = ObtenerViewModelDesdeSesion();

        if (archivoCarga is null || archivoCarga.Length == 0)
        {
            vm.Error = "Selecciona un archivo de cargas antes de continuar.";
            return View("Index", vm);
        }

        try
        {
            vm.CargasAcademicas =
                await _programacionAcademicaService
                    .ProcesarCargasAsync(
                        archivoCarga,
                        ObtenerOfertasSesion());
        }
        catch (Exception ex)
        {
            vm.Error = ex.Message;
        }

        return View("CargarProgramacionAcademicaPaso1", vm);
    }

    [HttpPost]
    public async Task<IActionResult> Guardar()
    {
        var ofertas = ObtenerOfertasSesion();

        if (ofertas.Count == 0)
            return BadRequest("No hay ofertas para guardar.");

        try
        {
            const int periodoId = 3;
            const int articuloId = 1;

            var guardado =
                await _programacionAcademicaService
                    .GuardarOfertasAsync(ofertas, periodoId, articuloId);
            return Ok(
                guardado
                    ? "Ofertas guardadas exitosamente."
                    : "No se pudo hacer el registro");
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                $"Error al guardar las ofertas: {ex.Message}");
        }
    }

    private List<OfertaDTO> ObtenerOfertasSesion()
    {
        var json = HttpContext.Session.GetString("Ofertas");

        return string.IsNullOrEmpty(json)
            ? []
            : JsonSerializer.Deserialize<List<OfertaDTO>>(json)!;
    }

    private CargarProgramacionAcademica2ViewModel ObtenerViewModelDesdeSesion()
    {
        var ofertas = ObtenerOfertasSesion();

        return new CargarProgramacionAcademica2ViewModel
        {
            OfertasVacantes = ofertas
                .Where(o => o.NP == null)
                .ToList(),

            OfertasAsignadas = ofertas
                .Where(o => o.NP != null)
                .ToList()
        };
    }

    [HttpPost]
    public async Task<IActionResult> ValidarPaso1(CargarProgramacionAcademica1ViewModel modelo)
    {
        if (modelo.ArchivoVacantes == null || modelo.ArchivoVacantes.Length == 0)
        {
            TempData["Error"] = "Selecciona el archivo de vacantes antes de continuar.";
            return RedirectToAction("CargarProgramacionAcademicaPaso1");
        }

        if (modelo.ArchivoDescargas == null || modelo.ArchivoDescargas.Length == 0)
        {
            TempData["Error"] = "Selecciona el archivo de descargas antes de continuar.";
            return RedirectToAction("CargarProgramacionAcademicaPaso1");
        }

        try
        {
            var ofertasVacantes = await _programacionAcademicaService.ProcesarArchivoAsync(modelo.ArchivoVacantes);
            var ofertasDescargas = await _programacionAcademicaService.ProcesarArchivoAsync(modelo.ArchivoDescargas);

            var todas = new List<OfertaDTO>();
            todas.AddRange(ofertasVacantes);
            todas.AddRange(ofertasDescargas);

            HttpContext.Session.SetString("Ofertas", JsonSerializer.Serialize(todas));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al procesar los archivos: {ex.Message}";
            return RedirectToAction("CargarProgramacionAcademicaPaso1");
        }

        return RedirectToAction("CargarProgramacionAcademicaPaso2");
    }

    [HttpGet]
    public IActionResult CargarProgramacionAcademicaPaso2()
    {
        var vm = ObtenerViewModelDesdeSesion();
        return View("CargarProgramacionAcademicaPaso2", vm);
    }


    // Ajax

    [HttpGet]
    public async Task<IActionResult> ObtenerEntidades(string region)
    {
        var entidades = await _programacionAcademicaService.ObtenerOpcionesEntidadAcademicaAsync(region);
        var result = entidades.Select(e => new { value = e.IdEntidadAcademica, text = e.Nombre });
        return Json(result);
    }

    //private async Task CargarCombos(CrearProgramaEducativoViewModel model)
    //{
    //model.Regiones = Constantes.REGIONES
    //    .Select(r => new OptionModel { Value = r, Text = r, Selected = r == model.Region })
    //    .ToList();

    //var areas = await _programaEducativoService.ObtenerOpcionesAreaAcademicaAsync();
    //model.Areas = areas
    //    .Select(a => new OptionModel
    //    {
    //        Value = a.IdAreaAcademica.ToString(),
    //        Text = a.Nombre,
    //        Selected = model.IdAreaAcademica.HasValue && a.IdAreaAcademica == model.IdAreaAcademica.Value
    //    })
    //    .ToList();

    //if (model.IdAreaAcademica.HasValue && !string.IsNullOrEmpty(model.Region))
    //{
    //    var entidades = await _entidadAcademicaRepository
    //        .ObtenerPorIdAreaAcademicaYRegionAsync(model.IdAreaAcademica.Value, model.Region);

    //    model.Entidades = entidades
    //        .Select(e => new OptionModel
    //        {
    //            Value = e.IdEntidadAcademica.ToString(),
    //            Text = e.Nombre,
    //            Selected = model.IdEntidadAcademica > 0 && e.IdEntidadAcademica == model.IdEntidadAcademica
    //        })
    //        .ToList();
    //}
    //else
    //{
    //    model.Entidades = new List<OptionModel>();
    //}
    //    }
}