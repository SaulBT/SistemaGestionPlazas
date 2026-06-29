namespace SGPla.Models.DTOs.Aviso
{
    public class FiltroAvisosDTO
    {
        public string Busqueda { get; set; } = string.Empty;
        public int IdPeriodo { get; set; }
        public int IdEntidadAcademica { get; set; }
        public string FechaInicio { get; set; } = string.Empty;

        public int Pagina { get; set; } = 1;
        public int Cantidad { get; set; } = 10;
    }
}
