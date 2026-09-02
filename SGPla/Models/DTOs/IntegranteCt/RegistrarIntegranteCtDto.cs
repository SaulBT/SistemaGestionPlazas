using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.DTOs.IntegranteCt
{
    public class RegistrarIntegranteCtDto
    {
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Cargo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Campo obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Campo obligatorio")]
        public string Grado { get; set; } = string.Empty;

        public int IdEntidadAcademica { get; set; }
    }
}
