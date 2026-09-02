namespace SGPla.Models.DTOs.PeriodoEscolar
{
    public class DetallesPeriodoEscolarDTO
    {

        public int IdPeriodoEscolar { get; set; }
        public string Codigo { get; set; } = string.Empty;

        public string Periodo { get; set; } = string.Empty;
        public int Anio { get; set; }

        public string PeriodoMostrar { get; set; }
    }
}
