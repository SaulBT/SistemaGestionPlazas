using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.DTOs.AreaAcademica
{
    public class CrearAreaAcademicaDTO
    {
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Extension { get; set; }
    }
}
