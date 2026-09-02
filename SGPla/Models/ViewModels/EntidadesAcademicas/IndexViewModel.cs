using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.EntidadesAcademicas
{
    public class IndexViewModel
    {
        public TableModel Table { get; set; }
        public List<OptionModel> Regiones { get; set; }
        public List<OptionModel> Areas { get; set; }

        public string? RegionSeleccionada { get; set; }
        public int? idAreaSeleccionada { get; set; }
        public string? Busqueda { get; set; }

        //paginación
        public int PaginaActual { get; set; } = 1;
        public int CantidadPorPagina { get; set; } = 10;
    }
}
