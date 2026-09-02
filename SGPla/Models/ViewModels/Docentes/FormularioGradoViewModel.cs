using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.Docentes
{
    public class FormularioGradoViewModel
    {
        public int IdGrado { get; set; }
        public int IdTemporal { get; set; }
        public List<OptionModel> ListaGrados { get; set; } = [];
        public string Area { get; set; } = string.Empty;
        public bool Ultimo { get; set; } = false;
    }
}
