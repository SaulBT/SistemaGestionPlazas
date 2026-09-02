using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SGPla.Models.Components;
using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.ViewModels.EntidadesAcademicas
{
    public class CrearEntidadAcademicaViewModel
    {
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Clave { get; set; } = string.Empty;

        [Required(ErrorMessage = "Campo obligatorio")]
        public string Nombre { get; set; } = string.Empty;
        [Required(ErrorMessage = "Campo obligatorio")]
        public string CalleNumero { get; set; } = string.Empty;
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Colonia { get; set; } = string.Empty;
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Cp { get; set; } = string.Empty;
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Municipio { get; set; } = string.Empty;
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Telefono { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Extension { get; set; } = string.Empty;

        [Required(ErrorMessage = "Campo obligatorio")]
        public int? IdAreaAcademica { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        public string Region { get; set; }

        public int IdEntidadAcademica { get; set; }

        // 🔹 Combos (para la vista)
        [ValidateNever]
        public List<OptionModel> Regiones { get; set; }

        [ValidateNever]
        public List<OptionModel> Areas { get; set; }
    }
}
