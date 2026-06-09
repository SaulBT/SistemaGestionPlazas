namespace SGPla.Models.DTOs.IntegranteCt
{
    public class RegistrarIntegranteCtDto
    {
        public string Cargo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Grado { get; set; } = string.Empty;
        public int IdEntidadAcademica { get; set; }
    }
}
