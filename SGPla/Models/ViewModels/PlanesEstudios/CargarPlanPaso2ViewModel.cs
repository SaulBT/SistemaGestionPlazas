using SGPla.Models.DTOs.PlanEstudios;
using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.ViewModels.PlanesEstudios
{
    public class CargarPlanPaso2ViewModel
    {
        public string Region { get; set; } = string.Empty;
        public int IdAreaAcademica { get; set; }
        public string NombreArea { get; set; } = string.Empty;
        public int IdEntidadAcademica { get; set; }
        public string NombrePrograma { get; set; } = string.Empty;
        public int IdProgramaEducativo { get; set; }
        public string Plan { get; set; } = string.Empty;
        public string Sistema { get; set; } = string.Empty;
        public FormularioExperienciaEducativaViewModel Formulario { get; set; }
        public IFormFile Archivo { get; set; }
        public TableModel Table { get; set; } = new TableModel();
        public bool Recarga { get; set; } = false;
    }
}
