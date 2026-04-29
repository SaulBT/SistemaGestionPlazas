using static SGPla.Mappers.PeriodoEscolarMapper;

namespace SGPla.Models.DTOs.PeriodoEscolar
{
    public class BuscarPeriodoEscolarDTO
    {
        public int? Anio { get; set; } 
        public string? Periodo { get; set; } = string.Empty;

        public int Pagina { get; set; } = 1;

        public int Cantidad { get; set; } = 10;

        public int? AnioCodigo { get; set; }
        public string? PeriodoCodigo {  get; set; } = string.Empty;
    }
}
