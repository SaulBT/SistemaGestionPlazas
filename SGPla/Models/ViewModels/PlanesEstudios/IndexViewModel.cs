using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.PlanesEstudios
{
    public class IndexViewModel
    {
        public TableModel Table { get; set; } = new TableModel();
        public List<OptionModel> Regiones { get; set; } = [];
        public List<OptionModel> Areas { get; set; } = [];
        public List<OptionModel> Entidades { get; set; } = [];
        public List<OptionModel> ProgramasEducativos { get; set; } = [];

        public int PaginaActual { get; set; } = 1;
        public int CantidadPorPaginas { get; set; } = 10;
    }
}
