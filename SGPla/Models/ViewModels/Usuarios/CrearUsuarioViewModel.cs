using SGPla.Models.Components;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SGPla.Models.ViewModels.Usuarios
{
    public class CrearUsuarioViewModel
    {
        // 🔹 Datos del formulario
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El cargo es obligatorio")]
        public string Cargo { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        public string Rol { get; set; }

        public int? IdAreaAcademica { get; set; }

        public string? Region { get; set; }

        public int? IdEntidadAcademica { get; set; }
        public int IdUsuario { get; set; }

        // 🔹 Combos (para la vista)
        [ValidateNever]
        public List<OptionModel> Regiones { get; set; }

        [ValidateNever]
        public List<OptionModel> Areas { get; set; }

        [ValidateNever]
        public List<OptionModel> Entidades { get; set; }

        [ValidateNever]
        public List<OptionModel> Roles { get; set; }
    }
}