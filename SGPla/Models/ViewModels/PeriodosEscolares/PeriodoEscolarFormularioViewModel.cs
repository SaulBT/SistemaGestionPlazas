using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.ViewModels.PeriodosEscolares
{
    public class PeriodoEscolarFormularioViewModel
    {

        public int? IdPeriodoEscolar { get; set; }

        [Required(ErrorMessage = "El año es obligatorio")]
        public int Anio { get; set; }

        [Required(ErrorMessage = "El periodo es obligatorio")]
        public string? Periodo { get; set; }
    }
}
