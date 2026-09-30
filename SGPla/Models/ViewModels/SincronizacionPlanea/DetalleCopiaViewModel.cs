using SGPla.Models.DTOs.Planea;

namespace SGPla.Models.ViewModels.SincronizacionPlanea
{
    public sealed class DetalleCopiaViewModel
    {
        public DetalleCopiaPlaneaDTO Detalle { get; set; } = null!;
        public TableModel TablaHorarios { get; set; } = new();
        public string UrlVolver { get; set; } = "/SincronizacionPlanea/Copias";
    }
}
