namespace SGPla.Models.DTOs.EntidadAcademica
{
    public class CrearEntidadAcademicaDTO
    {
        public int? IdAreaAcademica { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string CalleNumero { get; set; } = string.Empty;
        public string Colonia { get; set; } = string.Empty;
        public string Cp { get; set; } = string.Empty;
        public string Municipio { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }
}
