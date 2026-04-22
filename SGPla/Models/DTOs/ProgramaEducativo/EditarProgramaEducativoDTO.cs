using SGPla.Models.InterfacesDTOs;

namespace SGPla.Models.DTOs.ProgramaEducativo
{
    public class EditarProgramaEducativoDTO : IProgramaEducativoDTO
    {
        public int IdProgramaEducativo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int IdEntidadAcademica { get; set; }
    }
}
