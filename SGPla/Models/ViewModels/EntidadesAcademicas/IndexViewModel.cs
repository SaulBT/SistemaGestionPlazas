using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.EntidadesAcademicas
{
    public class IndexViewModel
    {
        public TableModel Table { get; set; }
        public List<OptionModel> Regiones { get; set; } = [];
        public List<OptionModel> Areas { get; set; } = [];

        public int? RegionIdSeleccionada { get; set; }
        public int? AreaAcademicaIdSeleccionada { get; set; }
        public string? Busqueda { get; set; }

        //paginación
        public int PaginaActual { get; set; } = 1;
        public int CantidadPorPagina { get; set; } = 10;
    }
}
