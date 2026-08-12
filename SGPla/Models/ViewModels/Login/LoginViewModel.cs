using SGPla.Models.Components;
using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.ViewModels.Login
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El campo correo es obligatorio")]
        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; } = null!;

        public string? ReturnUrl { get; set; }
    }
}

