using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.DTOs.AreaAcademica
{
    public class DatosAreaAcademicaDTO
    {
        [Required(ErrorMessage = "Campo obligatorio")]
        public int IdAreaAcademica { get; set; } = 0;
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Nombre { get; set; } = string.Empty;
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Telefono { get; set; } = string.Empty;
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Extension { get; set; } = string.Empty;
    }
}
