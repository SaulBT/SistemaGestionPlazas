using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.ViewModels.PeriodosEscolares
{
    public class PeriodoEscolarFormularioViewModel
    {

        public int? IdPeriodoEscolar { get; set; }

        [Range(2000, 2100, ErrorMessage = "Ingrese un año válido")]
        public int Anio { get; set; }

        [Required(ErrorMessage = "El periodo es obligatorio")]
        public string? Periodo { get; set; }
    }
}
