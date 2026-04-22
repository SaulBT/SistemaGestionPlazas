namespace SGPla.Models.DTOs.EntidadAcademica
{
    public class DatosEntidadAcademicaDTO
    {
        public int IdEntidadAcademica { get; set; }
        public int IdAreaAcademica { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string CalleNumero { get; set; } = string.Empty;
        public string Colonia { get; set; } = string.Empty;
        public string Cp { get; set; } = string.Empty;
        public string Municipio { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Conmutador { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
        public string NombreEntidadAcademica { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }
}
