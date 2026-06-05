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

    public ProgramacionesAcademicasController(ILogger<ProgramacionesAcademicasController> logger, IProgramacionAcademicaService programacionAcademicaService)
    {
        _logger = logger;
        _programacionAcademicaService = programacionAcademicaService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new CargarProgramacionAcademica2ViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Index(IFormFile archivo)
    {
        var vm = new CargarProgramacionAcademica2ViewModel();

        if (archivo is null || archivo.Length == 0)
        {
            vm.Error = "Selecciona un archivo antes de continuar.";
            return View(vm);
        }

        try
        {
            var ofertas = await _programacionAcademicaService.ProcesarArchivoAsync(archivo);
            HttpContext.Session.SetString(
                "Ofertas",
                JsonSerializer.Serialize(ofertas)
            );

            List<OfertaDTO> ofertasVacantes = new();
            List<OfertaDTO> ofertasAsignadas = new();

            foreach (OfertaDTO oferta in ofertas)
            {
                if (oferta.NP == null)
                    ofertasVacantes.Add(oferta);
                else
                    ofertasAsignadas.Add(oferta);
            }

            vm.OfertasVacantes = ofertasVacantes;
            vm.OfertasAsignadas = ofertasAsignadas;
        }
        catch (Exception ex)
        {
            vm.Error = $"Error al procesar el archivo: {ex.Message}";
        }

        return View(vm);
    }


    [HttpPost]
    public async Task<IActionResult> Guardar()
    {
        var json = HttpContext.Session.GetString("Ofertas");

        List<OfertaDTO> ofertas = string.IsNullOrEmpty(json)
            ? new List<OfertaDTO>()
            : JsonSerializer.Deserialize<List<OfertaDTO>>(json)!;

        if (ofertas.Count == 0)
            return BadRequest("No hay ofertas para guardar.");
        try
        {
            if (await _programacionAcademicaService.GuardarOfertasAsync(ofertas))
            return Ok("Ofertas guardadas exitosamente.");
            else
            {
                return Ok("No se pudo hacer el registro");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al guardar las ofertas: {ex.Message}");
        }
    }
}
