namespace SGPla.Models.DTOs.EntidadAcademica
{
    public class FiltroEntidadAcademicaDTO
    {
        public string Region { get; set; } = string.Empty;
        public int IdAreaAcademica { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public int Pagina { get; set; } = 1;

        public int Cantidad { get; set; } = 10;
    }
}
