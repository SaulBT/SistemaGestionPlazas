namespace SGPla.Models.DTOs.ProgramacionAcademica
{
    public class BuscarProgramacionAcademicaDTO
    {
        public string? Region { get; set; }
        public int? IdEntidadAcademica { get; set; }
        public int? IdProgramaEducativo { get; set; }
        public int? IdPeriodo { get; set; }
        public string? Busqueda { get; set; }
    }
}
