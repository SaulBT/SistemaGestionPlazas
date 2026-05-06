using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.DTOs.AreaAcademica
{
    public class CrearAreaAcademicaDTO
    {
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
        public string Conmutador { get; set; } = string.Empty;
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Extension { get; set; } = string.Empty;
        [Required(ErrorMessage = "Campo obligatorio")]
        public string Fax { get; set; } = string.Empty;

    }
}
