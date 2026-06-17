using DocumentFormat.OpenXml.Bibliography;
using SGPla.Models.Componentes;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.ProgramacionAcademica;

namespace SGPla.Models.ViewModels.ProgramacionesAcademicas;

public class CargarProgramacionAcademica2ViewModel
{
    
    public string NombrePeriodo { get; set; }
    public string NombreEntidadAcademica { get; set; }
    public string EntidadAcademicaSeleccionada { get; set; }
    public TableModel TableAsignadas { get; set; }
    public TableModel TableVacantes { get; set; }
    public List<OfertaDTO>? OfertasVacantes { get; set; }
    public List<OfertaDTO>? OfertasAsignadas { get; set; }
    public List<CargaConOfertaDTO>? CargasAcademicas { get; set; }
    public string? Error { get; set; }
    public List<OptionModel> Articulos { get; set; }
    public List<OptionModel> Programas { get; set; }
    public string? Programa { get; set; }
    public int? IdArticulo { get; set; }
    public string? Region { get; set; }
    public int? IdEntidadAcademica { get; set; } = 0;

    public int? IdPeriodo { get; set; } = 0;
}
