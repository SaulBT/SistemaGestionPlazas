namespace SGPla.Models.DTOs.PlanEstudios
{
    public class EditarPlanEstudiosDTO
    {
        public int IdPlanEstudios { get; set; }
        public bool NuevaLista { get; set; }
        public ArchivoPlanEstudiosDTO? Archivo { get; set; }
        public List<AgregarExperienciaEducativaDTO> ExperienciasNuevas = [];
        public List<DatosExperienciaEducativaDTO> ExperienciasEditadas = [];
        public List<int> IdsExperienciasEliminadas = [];
    }
}
