using SGPla.Models.DTOs.Planea;

namespace SGPla.Models.ViewModels.ProgramacionesAcademicas
{
    // Programación sincronizada desde PLANEA de un programa educativo en un periodo.
    public class ProgramacionPlaneaViewModel
    {
        public int IdProgramaEducativo { get; set; }
        public int IdPeriodo { get; set; }

        public string? Region { get; set; }
        public string? NombreEntidadAcademica { get; set; }
        public string? NombrePrograma { get; set; }
        public string? NombrePeriodo { get; set; }

        public UltimaSincronizacionPlaneaDTO? UltimaSincronizacionPlanea { get; set; }

        public TableModel TablaPlanea { get; set; } = new();
    }
}
