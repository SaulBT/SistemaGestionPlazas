using SGPla.Models.DTOs.ProgramacionAcademica;

namespace SGPla.Models.ViewModels.ProgramacionesAcademicas
{
    public class VerHistorialExperienciaEducativaViewModel
    {
        public string ExperienciaEducativa { get; set; }
        public List<LogDTO> Movimientos {  get; set; }
    }
}
