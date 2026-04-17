namespace SGPla.Models.DTOs.AreaAcademica
{
    public class DatosAreaAcademicaDTO
    {
        public int IdAreaAcademica { get; set; } = 0;
        public string Nombre { get; set; } = string.Empty;
        public string CalleNumero { get; set; } = string.Empty;
        public string Colonia { get; set; } = string.Empty;
        public string Cp { get; set; } = string.Empty;
        public string Municipio { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Conmutador { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
    }
}
