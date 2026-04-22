namespace SGPla.Models.DTOs.EntidadAcademica
{
    public class ListaEntidadAcademicaDTO
    {
        public int IdEntidadAcademica { get; set; }
        public int IdAreaAcademica { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Domicilio { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string NombreEntidadAcademica { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }
}
