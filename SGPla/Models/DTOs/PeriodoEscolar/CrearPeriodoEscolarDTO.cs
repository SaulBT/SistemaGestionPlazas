using SGPla.Models.InterfacesDTOs;

namespace SGPla.Models.DTOs.PeriodoEscolar
{
    public class CrearPeriodoEscolarDTO : IPeriodoEscolarDTO
    {
        public string Periodo { get; set; } = string.Empty;

        public int Anio { get; set; }
    }
}
