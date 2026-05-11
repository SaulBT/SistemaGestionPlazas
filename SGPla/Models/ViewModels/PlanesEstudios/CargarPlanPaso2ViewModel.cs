using SGPla.Models.DTOs.PlanEstudios;

namespace SGPla.Models.ViewModels.PlanesEstudios
{
    public class CargarPlanPaso2ViewModel
    {
        public string Region { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string ProgramaEducativo { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string Sistema { get; set; } = string.Empty;
        public List<DatosExperienciaEducativaDTO> Experiencias { get; set; } = [];
        public IFormFile Archivo { get; set; }
    }
}
