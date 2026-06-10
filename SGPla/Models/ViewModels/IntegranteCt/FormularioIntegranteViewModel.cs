using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.IntegranteCt
{
    public class FormularioIntegranteViewModel
    {
        public int IdIntegranteCt { get; set; }
        public string Cargo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public List<OptionModel> Grados { get; set; } = [];
    }
}
