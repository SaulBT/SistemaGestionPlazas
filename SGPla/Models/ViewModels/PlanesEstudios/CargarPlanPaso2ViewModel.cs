using SGPla.Models.DTOs.PlanEstudios;

namespace SGPla.Models.ViewModels.PlanesEstudios
{
    public class CargarPlanPaso2ViewModel
    {
        public string Region { get; set; } = string.Empty;
        public int Area { get; set; }
        public string NombreArea { get; set; } = string.Empty;
        public int Programa { get; set; }
        public string NombrePrograma { get; set; } = string.Empty;
        public int IdProgramaEducativo { get; set; }
        public string Plan { get; set; } = string.Empty;
        public string Sistema { get; set; } = string.Empty;
        public IFormFile Archivo { get; set; }
        public TableModel Table { get; set; }
    }
}
