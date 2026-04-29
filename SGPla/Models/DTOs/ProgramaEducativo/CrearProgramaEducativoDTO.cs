using SGPla.Models.InterfacesDTOs;

namespace SGPla.Models.DTOs.ProgramaEducativo
{
    public class CrearProgramaEducativoDTO : IProgramaEducativoDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public int IdEntidadAcademica { get; set; }
    }
}
