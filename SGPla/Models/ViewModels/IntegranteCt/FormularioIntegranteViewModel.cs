using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.IntegranteCt
{
    public class FormularioIntegranteViewModel
    {
        public int IdIntegranteCt { get; set; } = -1;
        public string Cargo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Grado { get; set; } = string.Empty;
        public List<OptionModel> Grados { get; set; } = [];
    }
}
