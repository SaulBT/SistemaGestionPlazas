using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.DTOs.AreaAcademica
{
    public class DatosAreaAcademicaDTO
    {
        [Required(ErrorMessage = "Campo obligatorio")]
        public int IdAreaAcademica { get; set; } = 0;
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Extension { get; set; }
    }
}
