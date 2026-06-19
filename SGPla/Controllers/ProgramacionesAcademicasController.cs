using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Commons.Factories;
using SGPla.Models;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Articulo;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.ViewModels.ProgramacionesAcademicas;
using SGPla.Services.Interfaces;
using System.Text.Json;

namespace SGPla.Controllers;


enum TipoTablaOferta
{
    Asignadas,
    Vacantes
}

public class ProgramacionesAcademicasController : Controller
{
    private readonly ILogger<ProgramacionesAcademicasController> _logger;
    private readonly IProgramacionAcademicaService _programacionAcademicaService;
    private readonly IPeriodoEscolarService _periodoEscolarService;
    private readonly IArticuloService _articuloService;
    private int _paginaActual = 1;
    private static List<string> HEADERS_TABLA_ASIGNADAS = ["Experiencia educativa", "NRC", "H/S/M", "Tipo contratación", "Horario", "Docente"];
    private static List<string> HEADERS_TABLA_VACANTES = ["Experiencia educativa", "NRC", "H/S/M", "Tipo contratación", "Horario", "Artículo"];
    private const string SESSION_REGION = "Region";
    private const string SESSION_ID_PERIODO = "IdPeriodo";
    private const string SESSION_ID_ENTIDAD = "IdEntidad";
    private const string SESSION_NOMBRE_PERIODO = "NombrePeriodo";
    private const string SESSION_NOMBRE_ENTIDAD = "NombreEntidadAcademica";

    public ProgramacionesAcademicasController(
        ILogger<ProgramacionesAcademicasController> logger,
        IProgramacionAcademicaService programacionAcademicaService,
        IPeriodoEscolarService periodoEscolarService,
        IArticuloService articuloService)
    {
        _logger = logger;
        _programacionAcademicaService = programacionAcademicaService;
        _periodoEscolarService = periodoEscolarService;
        _articuloService = articuloService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        IndexViewModel modelo = new IndexViewModel();
        modelo.Regiones = Constantes.REGIONES
            .Select(r => new OptionModel { Value = r, Text = r })
            .ToList();

        var periodos = await _periodoEscolarService.ObtenerTodosAsync();

        modelo.Periodos = periodos
            .Select(p => new OptionModel
            {
                Value = p.IdPeriodoEscolar.ToString(),
                Text = p.PeriodoMostrar,
            })
            .ToList();

        List<EntidadAcademica> entidades = [];

        if (!string.IsNullOrEmpty(modelo.Region))
        {
            entidades =
                await _programacionAcademicaService
                    .ObtenerOpcionesEntidadAcademicaAsync(modelo.Region);
        }

        modelo.Entidades = entidades
            .Select(e => new OptionModel
            {
                Value = e.IdEntidadAcademica.ToString(),
                Text = e.Nombre
            })
            .ToList();

        return View("Index", modelo);
    }

    [HttpGet]
    public async Task<IActionResult> CargarProgramacionAcademicaPaso1()
    {
        HttpContext.Session.Remove("Ofertas");
        HttpContext.Session.Remove("Cargas");

        CargarProgramacionAcademica1ViewModel modelo = new CargarProgramacionAcademica1ViewModel();
        await CargarCombos(modelo);

        return (View("CargarProgramacionAcademicaPaso1", modelo));
    }

    public async Task CargarCombos(CargarProgramacionAcademica1ViewModel modelo)
    {
        modelo.Regiones = Constantes.REGIONES
             .Select(r => new OptionModel { Value = r, Text = r })
             .ToList();

        var periodos = await _periodoEscolarService.ObtenerTodosAsync();

        modelo.Periodos = periodos
            .Select(p => new OptionModel
            {
                Value = p.IdPeriodoEscolar.ToString(),
                Text = p.PeriodoMostrar,
            })
            .ToList();

        List<EntidadAcademica> entidades = [];

        if (!string.IsNullOrEmpty(modelo.Region))
        {
            entidades =
                await _programacionAcademicaService
                    .ObtenerOpcionesEntidadAcademicaAsync(modelo.Region);
        }

        modelo.Entidades = entidades
            .Select(e => new OptionModel
            {
                Value = e.IdEntidadAcademica.ToString(),
                Text = e.Nombre
            })
            .ToList();
    }

    [HttpPost]
    public async Task<IActionResult> Guardar()
    {
        var ofertas = ObtenerOfertasSesion();

        if (ofertas.Count == 0)
            return BadRequest("No hay ofertas para guardar.");

        try
        {
            var guardado =
                await _programacionAcademicaService
                    .GuardarOfertasAsync(ofertas);
            TempData["Success"] = "Cambios guardados con éxito";
            return RedirectToAction(nameof(Index));

        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            var vm = await ObtenerViewModelCompletoAsync();
            return View("CargarProgramacionAcademicaPaso2", vm);
        }
    }

    private List<OfertaDTO> ObtenerOfertasSesion()
    {
        var json = HttpContext.Session.GetString("Ofertas");

        return string.IsNullOrEmpty(json)
            ? []
            : JsonSerializer.Deserialize<List<OfertaDTO>>(json)!;
    }

    [HttpGet]
    public async Task<IActionResult> FiltrarPrograma(string? programa)
    {
        var vm = await ObtenerViewModelCompletoAsync(programa);
        return View("CargarProgramacionAcademicaPaso2", vm);
    }

    private async Task<CargarProgramacionAcademica2ViewModel> ObtenerViewModelCompletoAsync(string? programa = null)
    {
        var vm = await ObtenerViewModelDesdeSesion(programa);

        vm.Region = HttpContext.Session.GetString(SESSION_REGION);
        vm.IdEntidadAcademica = HttpContext.Session.GetInt32(SESSION_ID_ENTIDAD)!.Value;
        vm.NombrePeriodo = HttpContext.Session.GetString(SESSION_NOMBRE_PERIODO);
        vm.NombreEntidadAcademica = HttpContext.Session.GetString(SESSION_NOMBRE_ENTIDAD);
        vm.ProgramaSeleccionado = programa;

        return vm;
    }

    private async Task<CargarProgramacionAcademica2ViewModel> ObtenerViewModelDesdeSesion(string? programa = null)
    {
        var ofertas = ObtenerOfertasSesion();

        var programasCombo = ofertas
         .Select(o => o.Programa)
         .Distinct()
         .OrderBy(p => p)
         .Select(p => new OptionModel
         {
             Value = p,
             Text = p
         })
         .ToList();

        HttpContext.Session.SetString(
            "Ofertas",
            JsonSerializer.Serialize(ofertas));

        var ofertasFiltradas = programa == null ? ofertas : ofertas.Where(o => o.Programa == programa);

        var ofertasAsignadas = ofertasFiltradas
            .Where(o => o.NP != null)
            .ToList();

        var ofertasVacantes = ofertasFiltradas
            .Where(o => o.NP == null)
            .ToList();

        IEnumerable<DetallesArticuloDTO> articulos = await _articuloService.ObtenerTodosAsync();

        var articulosCombo = articulos
           .Select(a => new OptionModel
           {
               Value = a.IdArticulo.ToString(),
               Text = a.Numero,
           })
           .ToList();

        return new CargarProgramacionAcademica2ViewModel
        {
            OfertasVacantes = ofertasVacantes,
            OfertasAsignadas = ofertasAsignadas,

            TableAsignadas = await LlenarTablaAsync(TipoTablaOferta.Asignadas, articulos, ofertasAsignadas, programa),
            TableVacantes = await LlenarTablaAsync(TipoTablaOferta.Vacantes, articulos, ofertasVacantes, programa),

            Articulos = articulosCombo,
            Programas = programasCombo
        };
    }

    private async Task<TableModel> LlenarTablaAsync(TipoTablaOferta tipoOferta, IEnumerable<DetallesArticuloDTO> articulos, List<OfertaDTO>? ofertas, string? programa)
    {
        try
        {
            return new TableModel
            {
                TableId = tipoOferta == TipoTablaOferta.Vacantes ? "tablaVacantes" : "tablaAsignadas",
                Headers = tipoOferta == TipoTablaOferta.Vacantes ? HEADERS_TABLA_VACANTES : HEADERS_TABLA_ASIGNADAS,
                Rows = ofertas.Select(oferta => new TableRowModel
                {
                    Cells = new List<TableCellModel>
                        {
                            new() { Value = oferta.ExperienciaEducativa },
                            new() { Value = oferta.NRC },
                            new() { Value = oferta.HorasPago.ToString() },
                            new() { Value = oferta.TC },
                            new() { Value = oferta.TC },

                            tipoOferta == TipoTablaOferta.Vacantes
                            ? new()
                            {
                                Value = articulos
                                .FirstOrDefault(a => a.IdArticulo == oferta.Articulo)?
                                .Numero ?? "—"
                            }
                            : new()
                            {
                                Value = oferta.NombreDocente
                            }
                        }
                }).ToList(),
                Pagination = new PaginationInfo
                {
                    CurrentPage = _paginaActual,
                    TotalItems = ofertas.Count,
                    OnPageChange = "cambiarPagina",
                    PaginationMode = "client"
                }
            };
        }
        catch (Exception)
        {
            return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_ASIGNADAS, string.Format(Constantes.ERROR_TABLA, Constantes.PLANES_ESTUDIOS));
        }
    }

    [HttpPost]
    public async Task<IActionResult> ValidarPaso1(CargarProgramacionAcademica1ViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            var erroresArchivos = ModelState
                .Where(x => x.Key == nameof(modelo.ArchivoVacantes)
                         || x.Key == nameof(modelo.ArchivoDescargas))
                .SelectMany(x => x.Value.Errors)
                .Select(e => e.ErrorMessage);

            TempData["Error"] = string.Join("|", erroresArchivos);

            await CargarCombos(modelo);
            return View("CargarProgramacionAcademicaPaso1", modelo);
        }

        try
        {
            var ofertasVacantes = await _programacionAcademicaService.ProcesarArchivoOfertasAsync(modelo.ArchivoVacantes, TipoArchivoOferta.Vacantes);
            var ofertasDescargas = await _programacionAcademicaService.ProcesarArchivoOfertasAsync(modelo.ArchivoDescargas, TipoArchivoOferta.Descargas);

            var todas = new List<OfertaDTO>();
            todas.AddRange(ofertasVacantes);
            todas.AddRange(ofertasDescargas);

            foreach (var oferta in todas)
            {
                oferta.IdPeriodo = modelo.IdPeriodo.Value;
            }

            HttpContext.Session.SetString("Ofertas", JsonSerializer.Serialize(todas));
            var periodoSeleccionado = (await _periodoEscolarService.ObtenerTodosAsync())
                .FirstOrDefault(p => p.IdPeriodoEscolar == modelo.IdPeriodo!.Value);
            var entidadSeleccionada = (await _programacionAcademicaService.ObtenerOpcionesEntidadAcademicaAsync(modelo.Region))
                .FirstOrDefault(e => e.IdEntidadAcademica == modelo.IdEntidadAcademica!.Value);

            HttpContext.Session.SetString(SESSION_REGION, modelo.Region);
            HttpContext.Session.SetInt32(SESSION_ID_PERIODO, modelo.IdPeriodo.Value);
            HttpContext.Session.SetString(SESSION_NOMBRE_PERIODO, periodoSeleccionado?.PeriodoMostrar ?? "");
            HttpContext.Session.SetInt32(SESSION_ID_ENTIDAD, modelo.IdEntidadAcademica.Value);
            HttpContext.Session.SetString(SESSION_NOMBRE_ENTIDAD, entidadSeleccionada?.Nombre ?? "");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al procesar los archivos: {ex.Message}";
            await CargarCombos(modelo);
            return View("CargarProgramacionAcademicaPaso1", modelo);
        }
        return await CargarProgramacionAcademicaPaso2();
    }

    [HttpGet]
    public async Task<IActionResult> CargarProgramacionAcademicaPaso2()
    {
        var region = HttpContext.Session.GetString(SESSION_REGION);
        var idPeriodo = HttpContext.Session.GetInt32(SESSION_ID_PERIODO);
        var idEntidadAcademica = HttpContext.Session.GetInt32(SESSION_ID_ENTIDAD);

        if (string.IsNullOrEmpty(region) || idPeriodo is null || idEntidadAcademica is null)
        {
            TempData["Error"] = "No hay información de la carga. Vuelve a iniciar el proceso.";
            return RedirectToAction(nameof(CargarProgramacionAcademicaPaso1));
        }

        var vm = await ObtenerViewModelCompletoAsync();
        return View("CargarProgramacionAcademicaPaso2", vm);
    }

    [HttpPost]
    public async Task<IActionResult> AsignarArticulo(int idArticulo)
    {
        var ofertas = ObtenerOfertasSesion();

        foreach (var oferta in ofertas)
            oferta.Articulo = idArticulo;

        HttpContext.Session.SetString("Ofertas", JsonSerializer.Serialize(ofertas));

        var vm = await ObtenerViewModelCompletoAsync();
        vm.IdArticulo = idArticulo;

        return View("CargarProgramacionAcademicaPaso2", vm);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerEntidades(string region)
    {
        var entidades = await _programacionAcademicaService.ObtenerOpcionesEntidadAcademicaAsync(region);
        var result = entidades.Select(e => new { value = e.IdEntidadAcademica, text = e.Nombre });
        return Json(result);
    }
}