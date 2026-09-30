using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.SincronizacionPlanea
{
    public sealed class CopiasViewModel
    {
        public TableModel Table { get; set; } = new();
        public List<OptionModel> Periodos { get; set; } = [];
        public List<OptionModel> Planes { get; set; } = [];
        public List<OptionModel> Regiones { get; set; } = [];
        public string? Busqueda { get; set; }
        public int? IdPeriodo { get; set; }
        public int? IdPlanEstudios { get; set; }
        public int? IdRegion { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int CantidadPorPagina { get; set; } = 20;
    }
}
