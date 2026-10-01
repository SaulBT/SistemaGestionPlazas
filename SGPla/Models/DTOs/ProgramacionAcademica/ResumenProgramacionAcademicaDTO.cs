namespace SGPla.Models.DTOs.ProgramacionAcademica
{
    public class ResumenOfertaProgramacionAcademicaDTO
    {
        public int IdProgramaEducativo { get; set; }
        public string ProgramaEducativo { get; set; } = string.Empty;

        public int IdEntidadAcademica { get; set; }
        public string EntidadAcademica { get; set; } = string.Empty;

        public int IdPeriodo { get; set; }
        public string CodigoPeriodo { get; set; } = string.Empty;
        public string PeriodoMostrar { get; set; } = string.Empty;

        public int EEAsignadas { get; set; }
        public int EEVacantes { get; set; }
        public int TotalEE { get; set; }

        // Indica si el programa ya tiene NRC sincronizados desde PLANEA en el periodo.
        public bool TieneProgramacionPlanea { get; set; }

        public string? Region { get; set; }
    }
}
