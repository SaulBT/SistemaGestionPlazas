using SGPla.Models.InterfacesDTOs;

namespace SGPla.Models.DTOs.ProgramaEducativo
{
    public class BuscarProgramaEducativoDTO 
    {
        public string? Nombre { get; set; } = string.Empty;
        public int? IdEntidadAcademica { get; set; }

        public int? IdAreaAcademica { get; set; }

        public string? Region { get; set; }

        public int Pagina { get; set; } = 1;

        public int Cantidad { get; set; } = 10;
    }
}
