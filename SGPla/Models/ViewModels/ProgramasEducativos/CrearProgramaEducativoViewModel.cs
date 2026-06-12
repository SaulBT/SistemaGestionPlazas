using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SGPla.Models.Components;
using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.ViewModels.ProgramasEducativos
{
    public class CrearProgramaEducativoViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }


        [Required(ErrorMessage = "El código es obligatorio")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Debe contener exactamente 5 dígitos")]
        public string Clave { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una entidad académica")]
        public int? IdEntidadAcademica { get; set; }

        public int? IdAreaAcademica { get; set; }

        public string? Region { get; set; }

        [Required(ErrorMessage = "El campus es obligatorio")]

        public string Campus { get; set; } 

        public int IdProgramaEducativo { get; set; }



        [ValidateNever]
        public List<OptionModel> Regiones { get; set; }

        [ValidateNever]
        public List<OptionModel> Areas { get; set; }

        [ValidateNever]
        public List<OptionModel> Entidades { get; set; }

       

    }
}
