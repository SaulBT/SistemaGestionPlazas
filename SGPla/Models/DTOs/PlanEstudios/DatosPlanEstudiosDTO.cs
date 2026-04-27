namespace SGPla.Models.DTOs.PlanEstudios
{
    public class DatosPlanEstudiosDTO
    {
        public int IdPlanEstudios { get; set; }
        public string NombreProgramaEducativo { get; set; } = string.Empty;
        public string Modalidad { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string NombreAreaAcademica { get; set; } = string.Empty;
        public List<DatosExperienciaEducativaDTO> ExperienciasEducativos { get; set; } = [];
    }
}
