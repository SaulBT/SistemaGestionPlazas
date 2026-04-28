namespace SGPla.Models.DTOs.PlanEstudios
{
    public class CrearPlanEstudiosDTO
    {
        public int IdProgramaEducativo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Sistema { get; set; } = string.Empty;
        public ArchivoPlanEstudiosDTO Archivo { get; set; } = null!;
        public List<AgregarExperienciaEducativaDTO> ExperienciasEducativas { get; set; } = [];
    }
}
