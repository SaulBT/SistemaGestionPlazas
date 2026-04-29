namespace SGPla.Models.DTOs.PlanEstudios
{
    public class ListaPlanEstudiosDTO
    {
        public int IdPlanEstudios { get; set; }
        public string NombreProgramaEducativo { get; set; } = string.Empty;
        public string Modalidad { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
    }
}
