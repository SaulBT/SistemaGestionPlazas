using SGPla.Models.InterfacesDTOs;

namespace SGPla.Models.DTOs.ProgramaEducativo
{
    public class BuscarProgramaEducativoDTO : IProgramaEducativoDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public int IdEntidadAcademica { get; set; }

        public int IdAreaAcademica { get; set; }

        public int IdRegion { get; set; }
    }
}
