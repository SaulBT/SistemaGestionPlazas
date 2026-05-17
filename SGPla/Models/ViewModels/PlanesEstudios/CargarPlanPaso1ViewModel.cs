using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.PlanesEstudios
{
    public class CargarPlanPaso1ViewModel
    {
        public string Region { get; set; } = string.Empty;
        public int? Area { get; set; }
        public int? Entidad { get; set; }
        public int? Programa { get; set; }
        public string Plan { get; set; } = string.Empty;
        public string Sistema { get; set; } = string.Empty;
        public IFormFile Archivo { get; set; }

        public List<OptionModel> ListaRegiones { get; set; } = [];
        public List<OptionModel> ListaAreas { get; set; } = [];
        public List<OptionModel> ListaEntidades { get; set; } = [];
        public List<OptionModel> ListaProgramas { get; set; } = [];
        public List<OptionModel> ListaSistema { get; set; } = [];
    }
}
