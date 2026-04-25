using SGPla.Models.Components;

namespace SGPla.Models.ViewModels
{
    public class UsuariosIndexViewModel
    {
        public TableModel Table { get; set; }
        public List<OptionModel> Regiones { get; set; }
        public List<OptionModel> Areas { get; set; }
        public List<OptionModel> Entidades { get; set; }
    }
}

