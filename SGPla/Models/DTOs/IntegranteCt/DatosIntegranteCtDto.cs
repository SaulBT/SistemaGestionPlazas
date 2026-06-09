namespace SGPla.Models.DTOs.IntegranteCt
{
    public class DatosIntegranteCtDto
    {
        public int IdIntegranteCt { get; set; }
        public string Cargo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Grado { get; set; } = string.Empty;
        public int IdEntidadAcademica { get; set; }
    }
}
