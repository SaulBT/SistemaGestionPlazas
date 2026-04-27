namespace SGPla.Models.DTOs.PlanEstudios
{
    public class EditarPlanEstudiosDTO
    {
        public int IdPlanEstudios { get; set; }
        public List<AgregarExperienciaEducativaDTO> ExperienciasNuevas = [];
        public List<DatosExperienciaEducativaDTO> ExperienciasEditadas = [];
        public List<int> IdsExperienciasEliminadas = [];
    }
}
