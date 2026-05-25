using Microsoft.AspNetCore.Mvc;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.ViewModels.ProgramacionesAcademicas;

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
}
