using SGPla.Models.Componentes;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.ProgramacionAcademica;

namespace SGPla.Models.ViewModels.ProgramacionesAcademicas;

public class CargarProgramacionAcademica2ViewModel
{
    public string NombrePeriodo { get; set; }
    public string NombreEntidadAcademica { get; set; }
    public TableModel TableAsignadas { get; set; }
    public TableModel TableVacantes { get; set; }
    public TableModel TableCargas { get; set; }
    public List<OfertaDTO>? OfertasVacantes { get; set; }
    public List<OfertaDTO>? OfertasAsignadas { get; set; }
    public IEnumerable<CargaConOfertaDTO>? CargasAcademicas { get; set; }
    public string? Error { get; set; }
    public List<OptionModel> Programas { get; set; }
    public List<OptionModel> Docentes { get; set; }
    public string? Region { get; set; }
    public int? IdEntidadAcademica { get; set; } = 0;
    public int? IdPeriodo { get; set; } = 0;

    public FiltroOfertaDTO FiltroOferta { get; set; } = new();
    public FiltroCargaDTO FiltroCarga { get; set; } = new();
    public int TabActivo { get; set; }
}