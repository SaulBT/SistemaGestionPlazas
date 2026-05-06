using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.PlanesEstudios
{
    public class CargarPlanEstudiosPaso1ViewModel
    {
        public string RegionSeleccionada { get; set; } = string.Empty;
        public int? AreaSeleccionada { get; set; }
        public int? EntidadSeleccionada { get; set; }
        public int? ProgramaSeleccionado { get; set; }
        public string Plan { get; set; } = string.Empty;
        public string SistemaSeleccionado { get; set; } = string.Empty;

        public List<OptionModel> ListaRegiones { get; set; } = [];
        public List<OptionModel> ListaAreas { get; set; } = [];
        public List<OptionModel> ListaEntidades { get; set; } = [];
        public List<OptionModel> ListaProgramas { get; set; } = [];
        public List<OptionModel> ListaSistema { get; set; } = [];
    }
}
