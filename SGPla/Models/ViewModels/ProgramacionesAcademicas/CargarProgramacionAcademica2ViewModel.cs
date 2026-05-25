using SGPla.Models.DTOs.Oferta;

namespace SGPla.Models.ViewModels.ProgramacionesAcademicas;

public class CargarProgramacionAcademica2ViewModel
{
    /// <summary>Resultado del parseo. Null si aún no se ha subido archivo.</summary>
    public List<OfertaDTO>? OfertasVacantes { get; set; }

    public List<OfertaDTO>? OfertasAsignadas { get; set; }


    /// <summary>Mensaje de error si el parseo falló.</summary>
    public string? Error { get; set; }

    /// <summary>True si ya se procesó un archivo (exitoso o no).</summary>
    public bool Procesado => OfertasVacantes != null || OfertasAsignadas != null || Error != null;
}
