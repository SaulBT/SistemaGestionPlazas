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
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al procesar los archivos: {ex.Message}";
            return RedirectToAction("CargarProgramacionAcademicaPaso1");
        }

        return RedirectToAction("CargarProgramacionAcademicaPaso2", new { region = modelo.Region, idPeriodo = modelo.IdPeriodo, idEntidadAcademica = modelo.IdEntidadAcademica });
    }

    [HttpGet]
    public async Task<IActionResult> CargarProgramacionAcademicaPaso2(string region, int idPeriodo, int idEntidadAcademica)
    {
        var vm = await ObtenerViewModelDesdeSesion(idPeriodo);
        vm.Region = region;
        vm.IdEntidadAcademica = idEntidadAcademica;



        return View("CargarProgramacionAcademicaPaso2", vm);
    }


    [HttpPost]
    public IActionResult AsignarArticulo(int idArticulo)
    {
        var ofertas = ObtenerOfertasSesion();

        foreach (var oferta in ofertas)
        {
            oferta.Articulo = idArticulo;
        }

        HttpContext.Session.SetString(
            "Ofertas",
            JsonSerializer.Serialize(ofertas));

        return Ok();
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