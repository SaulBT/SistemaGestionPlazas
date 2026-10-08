using SGPla.Models.Components;
using SGPla.Models.DTOs.Planea;

namespace SGPla.Models.ViewModels.ProgramacionesAcademicas
{
    // Programación sincronizada desde PLANEA de un plan de estudios en un periodo.
    public class ProgramacionPlaneaViewModel
    {
        public int IdPlanEstudios { get; set; }
        public int IdPeriodo { get; set; }

        public string? Region { get; set; }
        public string? NombreEntidadAcademica { get; set; }
        public string? NombrePrograma { get; set; }
        public string? CodigoPlan { get; set; }
        public string? NombrePeriodo { get; set; }

        public UltimaSincronizacionPlaneaDTO? UltimaSincronizacionPlanea { get; set; }

        public TableModel TablaPlanea { get; set; } = new();

        public ResumenAprobacionPlaneaDTO Resumen { get; set; } = new(0, 0, 0, null, null);
        public bool HayPendientes { get; set; }
        // Hora de México de la última confirmación de DGAA.
        public DateTime? UltimaRevisionLocal { get; set; }
    }
}
