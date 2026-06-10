using Microsoft.AspNetCore.Mvc;
using SGPla.Models;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.ViewModels.ProgramacionesAcademicas;
using System.Text.Json;

namespace SGPla.Controllers;

public class ProgramacionesAcademicasController : Controller
{
    private readonly ILogger<ProgramacionesAcademicasController> _logger;
    private readonly IProgramacionAcademicaService _programacionAcademicaService;

    public ProgramacionesAcademicasController(
        ILogger<ProgramacionesAcademicasController> logger,
        IProgramacionAcademicaService programacionAcademicaService)
    {
        _logger = logger;
        _programacionAcademicaService = programacionAcademicaService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        HttpContext.Session.Remove("Ofertas");
        HttpContext.Session.Remove("Cargas");
        return View(new CargarProgramacionAcademica2ViewModel());
    }

    

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
            return View("Index", vm);
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

        return View("Index", vm);
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

        return View("Index", vm);
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
}