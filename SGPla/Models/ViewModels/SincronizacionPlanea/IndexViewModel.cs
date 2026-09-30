using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.SincronizacionPlanea
{
    public sealed class IndexViewModel
    {
        public TableModel Table { get; set; } = new();
        public List<OptionModel> Periodos { get; set; } = [];
        public List<OptionModel> Estados { get; set; } = [];
        public int? IdPeriodo { get; set; }
        public string? Estado { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int CantidadPorPagina { get; set; } = 10;
    }
}
