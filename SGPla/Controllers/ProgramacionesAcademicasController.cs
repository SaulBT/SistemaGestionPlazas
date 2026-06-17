using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Articulo;
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
    public async Task<IActionResult> CargarProgramacionAcademicaPaso1()
    {
        HttpContext.Session.Remove("Ofertas");
        HttpContext.Session.Remove("Cargas");

        var regionesCombo = Constantes.REGIONES
               .Select(r => new OptionModel { Value = r, Text = r })
               .ToList();

        var periodos = await _periodoEscolarService.ObtenerTodosAsync();

        var periodosCombo = periodos
            .Select(p => new OptionModel
            {
                Value = p.IdPeriodoEscolar.ToString(),
                Text = p.PeriodoMostrar,
            })
            .ToList();

        List<EntidadAcademica> entidades;

        entidades = new List<EntidadAcademica>();


        var entidadesCombo = entidades
            .Select(e => new OptionModel
            {
                Value = e.IdEntidadAcademica.ToString(),
                Text = e.Nombre,
            })
            .ToList();



        return View(new CargarProgramacionAcademica1ViewModel
        {
            Regiones = regionesCombo,
            Entidades = entidadesCombo,
            Periodos = periodosCombo,
        });
    }


    //[HttpPost]
    //public async Task<IActionResult> CargarCargas(IFormFile archivoCarga)
    //{
    //    var vm = ObtenerViewModelDesdeSesion();

    //    if (archivoCarga is null || archivoCarga.Length == 0)
    //    {
    //        vm.Error = "Selecciona un archivo de cargas antes de continuar.";
    //        return View("Index", vm);
    //    }

    //    try
    //    {
    //        vm.CargasAcademicas =
    //            await _programacionAcademicaService
    //                .ProcesarCargasAsync(
    //                    archivoCarga,
    //                    ObtenerOfertasSesion());

    //    }
    //    catch (Exception ex)
    //    {
    //        vm.Error = ex.Message;
    //    }

    //    return View("CargarProgramacionAcademicaPaso1", vm);
    //}

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

    private async Task<CargarProgramacionAcademica2ViewModel> ObtenerViewModelDesdeSesion(int idPeriodo)
    {
        var ofertas = ObtenerOfertasSesion();

        foreach (var oferta in ofertas)
        {
            oferta.IdPeriodo = idPeriodo;
        }

        HttpContext.Session.SetString(
            "Ofertas",
            JsonSerializer.Serialize(ofertas));

        var ofertasAsignadas = ofertas
                .Where(o => o.NP != null)
                .ToList();

        var ofertasVacantes = ofertas
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
            OfertasVacantes = ofertas
                .Where(o => o.NP == null)
                .ToList(),

            OfertasAsignadas = ofertas
                .Where(o => o.NP != null)
                .ToList(),

            TableAsignadas = new TableModel
            {
                TableId = "tablaAsignadas",
                Headers = HEADERS_TABLA_ASIGNADAS,
                Rows = ofertasAsignadas.Select(oferta => new TableRowModel
                {
                    Cells = new List<TableCellModel>
                        {
                            new() { Value = oferta.ExperienciaEducativa },
                            new() { Value = oferta.NRC },
                            new() { Value = oferta.HorasPago.ToString() },
                            new() { Value = oferta.TC },
                            new() { Value = oferta.TC },
                            new() { Value = oferta.NombreDocente }
                        }
                }).ToList(),
                Pagination = new PaginationInfo
                {
                    CurrentPage = _paginaActual,
                    TotalItems = ofertas.Count,
                    OnPageChange = "cambiarPagina",
                    PaginationMode = "client"
                }
            },
            TableVacantes = new TableModel
            {
                TableId = "tablaVacantes",

                Headers = HEADERS_TABLA_VACANTES,
                Rows = ofertasVacantes.Select(oferta => new TableRowModel
                {
                    Cells = new List<TableCellModel>
                        {
                            new() { Value = oferta.ExperienciaEducativa },
                            new() { Value = oferta.NRC },
                            new() { Value = oferta.HorasPago.ToString() },
                            new() { Value = oferta.TC },
                            new() { Value = oferta.TC },
                            new()
                            {
                                Value = articulos
                                    .FirstOrDefault(a => a.IdArticulo == oferta.Articulo)?
                                    .Numero ?? "—"
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
            },
            Articulos = articulosCombo
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

        if (string.IsNullOrEmpty(modelo.Region))
        {
            TempData["Error"] = "Selecciona una región antes de continuar.";
            return RedirectToAction("CargarProgramacionAcademicaPaso1");
        }

        if (modelo.IdPeriodo == null)
        {
            TempData["Error"] = "Selecciona un periodo escolar antes de continuar.";
            return RedirectToAction("CargarProgramacionAcademicaPaso1");
        }

        if (modelo.IdEntidadAcademica == null)
        {
            TempData["Error"] = "Selecciona una entidad académica antes de continuar.";
            return RedirectToAction("CargarProgramacionAcademicaPaso1");
        }

        try
        {
            var ofertasVacantes = await _programacionAcademicaService.ProcesarArchivoOfertasAsync(modelo.ArchivoVacantes, TipoArchivoOferta.Vacantes);
            var ofertasDescargas = await _programacionAcademicaService.ProcesarArchivoOfertasAsync(modelo.ArchivoDescargas, TipoArchivoOferta.Descargas);

            var todas = new List<OfertaDTO>();
            todas.AddRange(ofertasVacantes);
            todas.AddRange(ofertasDescargas);

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
            return RedirectToAction("CargarProgramacionAcademicaPaso1");
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

        var vm = await ObtenerViewModelDesdeSesion(idPeriodo.Value);
        vm.Region = region;
        vm.IdEntidadAcademica = idEntidadAcademica.Value;
        vm.NombrePeriodo = HttpContext.Session.GetString(SESSION_NOMBRE_PERIODO);
        vm.NombreEntidadAcademica = HttpContext.Session.GetString(SESSION_NOMBRE_ENTIDAD);

        return View("CargarProgramacionAcademicaPaso2", vm);
    }

    [HttpPost]
    public async Task<IActionResult> AsignarArticulo(int idArticulo)
    {
        var ofertas = ObtenerOfertasSesion();

        foreach (var oferta in ofertas)
            oferta.Articulo = idArticulo;

        HttpContext.Session.SetString("Ofertas", JsonSerializer.Serialize(ofertas));

        var idPeriodo = HttpContext.Session.GetInt32(SESSION_ID_PERIODO)!.Value;
        var vm = await ObtenerViewModelDesdeSesion(idPeriodo);
        vm.Region = HttpContext.Session.GetString(SESSION_REGION);
        vm.IdEntidadAcademica = HttpContext.Session.GetInt32(SESSION_ID_ENTIDAD)!.Value;
        vm.NombrePeriodo = HttpContext.Session.GetString(SESSION_NOMBRE_PERIODO);
        vm.NombreEntidadAcademica = HttpContext.Session.GetString(SESSION_NOMBRE_ENTIDAD);
        vm.IdArticulo = idArticulo; // ← esto

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
}