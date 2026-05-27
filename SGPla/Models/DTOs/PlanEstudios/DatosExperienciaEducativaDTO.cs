namespace SGPla.Models.DTOs.PlanEstudios
{
    public class DatosExperienciaEducativaDTO
    {
        public int IdExperienciaEducativa { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string PerfilDocente { get; set; } = string.Empty;
        public int Horas { get; set; }
        public int Creditos { get; set; }
    }
}
