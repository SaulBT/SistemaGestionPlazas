using SGPla.Models.InterfacesDTOs;

namespace SGPla.Models.DTOs.ProgramaEducativo
{
    public class CrearProgramaEducativoDTO : IProgramaEducativoDTO
    {
        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Campus { get; set; } = string.Empty;
        public int IdEntidadAcademica { get; set; }
    }
}
