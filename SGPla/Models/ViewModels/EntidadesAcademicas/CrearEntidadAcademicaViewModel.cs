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
        public string Calle { get; set; } = string.Empty;
        public string? NumeroExterior { get; set; }
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Colonia { get; set; } = string.Empty;
        [Required(ErrorMessage = "Campo obligatorio")]
        public string CodigoPostal { get; set; } = string.Empty;
        [Required(ErrorMessage = "Seleccione un municipio")]
        public int? MunicipioId { get; set; }
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Telefono { get; set; } = string.Empty;
        
        public string? Extension { get; set; }

        [Required(ErrorMessage = "Seleccione un área académica")]
        public int? AreaAcademicaId { get; set; }
        public int? RegionId { get; set; }

        [Required(ErrorMessage = "Seleccione un campus")]
        public int? CampusId { get; set; }

        public int IdEntidadAcademica { get; set; }

        // 🔹 Combos (para la vista)
        [ValidateNever]
        public List<OptionModel> Regiones { get; set; } = [];

        [ValidateNever]
        public List<OptionModel> Campus { get; set; } = [];

        [ValidateNever]
        public List<OptionModel> Areas { get; set; } = [];

        [ValidateNever]
        public List<OptionModel> Municipios { get; set; } = [];
    }
}
