using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.Usuarios
{
    public class IndexViewModel
    {
        public TableModel Table { get; set; }
        public List<OptionModel> Regiones { get; set; }
        public List<OptionModel> Areas { get; set; }
        public List<OptionModel> Entidades { get; set; }

        public string? RegionSeleccionada { get; set; }
        public int? IdAreaSeleccionada { get; set; }
        public int? IdEntidadSeleccionada { get; set; }
        public string? Busqueda { get; set; }
    }
}

