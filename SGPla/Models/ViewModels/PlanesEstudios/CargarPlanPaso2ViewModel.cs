using SGPla.Models.DTOs.PlanEstudios;
using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.ViewModels.PlanesEstudios
{
    public class CargarPlanPaso2ViewModel
    {
        public string Region { get; set; } = string.Empty;
        public int Area { get; set; }
        public string NombreArea { get; set; } = string.Empty;
        public int Entidad { get; set; }
        public string NombrePrograma { get; set; } = string.Empty;
        public int Programa { get; set; }
        public string Plan { get; set; } = string.Empty;
        public string Sistema { get; set; } = string.Empty;
        public FormularioExperienciaEducativaViewModel Formulario { get; set; }
        public IFormFile Archivo { get; set; }
        public TableModel Table { get; set; }
    }
}
