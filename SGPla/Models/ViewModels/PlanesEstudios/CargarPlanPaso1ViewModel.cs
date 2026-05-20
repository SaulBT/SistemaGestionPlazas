using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SGPla.Models.Components;
using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.ViewModels.PlanesEstudios
{
    public class CargarPlanPaso1ViewModel
    {
        [Required(ErrorMessage = "Selecciona una Región.")]
        public string? Region { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona un Área")]
        public int? IdAreaAcademica { get; set; }

        [Required(ErrorMessage = "Selecciona una Entidad")]
        public int? IdEntidadAcademica { get; set; }

        [Required(ErrorMessage = "Selecciona un Programa")]
        public int? IdProgramaEducativo { get; set; }

        [Required(ErrorMessage = "Ingresa un nombre para el Plan")]
        public string Plan { get; set; }

        [Required(ErrorMessage = "Selecciona un Sistema")]
        public string Sistema { get; set; } = string.Empty;
        public IFormFile Archivo { get; set; }

        [ValidateNever]
        public List<OptionModel> ListaRegiones { get; set; } = [];
        [ValidateNever]
        public List<OptionModel> ListaAreas { get; set; } = [];
        [ValidateNever]
        public List<OptionModel> ListaEntidades { get; set; } = [];
        [ValidateNever]
        public List<OptionModel> ListaProgramas { get; set; } = [];
        [ValidateNever]
        public List<OptionModel> ListaSistema { get; set; } = [];
    }
}
