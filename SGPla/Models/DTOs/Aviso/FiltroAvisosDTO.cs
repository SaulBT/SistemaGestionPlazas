namespace SGPla.Models.DTOs.Aviso
{
    public class FiltroAvisosDTO
    {
        public string? Busqueda { get; set; } = string.Empty;
        public int? IdPeriodo { get; set; }
        public int? IdEntidadAcademica { get; set; }
        public int? IdAreaAcademica { get; set; }
        public DateOnly? FechaInicio { get; set; }

        public int Pagina { get; set; } = 1;
        public int Cantidad { get; set; } = 10;
    }
}
