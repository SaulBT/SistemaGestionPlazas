using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.ProgramacionAcademica;

namespace SGPla.Models.ViewModels.ProgramacionesAcademicas;

public class CargarProgramacionAcademica2ViewModel
{
    public List<OfertaDTO>? OfertasVacantes { get; set; }

    public List<OfertaDTO>? OfertasAsignadas { get; set; }

    public List<CargaConOfertaDTO>? CargasAcademicas { get; set; }

    public string? Error { get; set; }

    public bool Procesado => OfertasVacantes != null || OfertasAsignadas != null || Error != null;
}
