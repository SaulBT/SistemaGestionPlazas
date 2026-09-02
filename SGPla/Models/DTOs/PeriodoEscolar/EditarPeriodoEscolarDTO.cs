using SGPla.Models.InterfacesDTOs;

namespace SGPla.Models.DTOs.PeriodoEscolar
{
    public class EditarPeriodoEscolarDTO : IPeriodoEscolarDTO
    {
        public int IdPeriodoEscolar { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Periodo { get; set; } = string.Empty;
        public string Anio { get; set; } 
    }
}
