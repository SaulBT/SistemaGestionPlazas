namespace SGPla.Models.DTOs.ProgramaEducativo
{
    public class DetallesProgramaEducativoDTO
    {
        public int IdProgramaEducativo { get; set; }

        public string Nombre { get; set; } = null!;

        public int IdEntidadAcademica { get; set; }

        public string Region { get; set; } = string.Empty;

        public int IdAreaAcademica { get; set; }

        public string EntidadAcademica { get; set; } = string.Empty;

        public string AreaAcademica { get; set; } = string.Empty;

    }
}
