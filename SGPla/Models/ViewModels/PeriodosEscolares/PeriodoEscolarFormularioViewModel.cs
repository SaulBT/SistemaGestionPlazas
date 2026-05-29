using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.ViewModels.PeriodosEscolares
{
    public class PeriodoEscolarFormularioViewModel
    {

        public int? IdPeriodoEscolar { get; set; }

        [Required(ErrorMessage = "El año de ejercicio es obligatorio")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Ingrese un año válido")]
        public string? Anio { get; set; }

        [Required(ErrorMessage = "El periodo es obligatorio")]
        public string? Periodo { get; set; }
    }
}
