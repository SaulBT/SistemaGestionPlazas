using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.ViewModels.Login;

public sealed class CambiarContrasenaSuperusuarioViewModel
{
    [Required(ErrorMessage = "Capture su contraseña actual.")]
    [DataType(DataType.Password)]
    public string ContrasenaActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Capture una contraseña nueva.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "La contraseña nueva debe tener entre 8 y 128 caracteres.")]
    [DataType(DataType.Password)]
    public string ContrasenaNueva { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme la contraseña nueva.")]
    [Compare(nameof(ContrasenaNueva), ErrorMessage = "Las contraseñas nuevas no coinciden.")]
    [DataType(DataType.Password)]
    public string ConfirmarContrasena { get; set; } = string.Empty;
}
