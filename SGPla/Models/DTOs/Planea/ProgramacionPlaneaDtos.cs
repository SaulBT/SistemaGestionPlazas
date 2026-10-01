namespace SGPla.Models.DTOs.Planea
{
    public sealed class FiltroProgramacionPlaneaDTO
    {
        public int? IdPeriodo { get; set; }
        public int? IdEntidadAcademica { get; set; }
        public int? IdProgramaEducativo { get; set; }
        public int? IdPlanEstudios { get; set; }
        public string? Busqueda { get; set; }
        public int Pagina { get; set; } = 1;
        public int Limite { get; set; } = 10;
    }

    public sealed record UltimaSincronizacionPlaneaDTO(
        string CodigoPeriodo, string Estado, DateTime FechaInicio, DateTime? FechaFin,
        int? NrcRecibidos, int? NrcNuevos, int? NrcExistentes, int? NrcSinPlan, int? NrcSinExperiencia,
        int? HorariosInsertados, string? MensajeError);

    public sealed record HorarioPlaneaDTO(string Dia, TimeOnly HoraInicio, TimeOnly HoraFin, string? Edificio, string? Aula);

    /// Docente del NRC según PLANEA; Imparte es null si PLANEA no lo indica.
    public sealed record DocentePlaneaDTO(string Nombre, bool? Imparte);

    public sealed record CopiaProgramacionPlaneaDTO(
        string CodigoPeriodo, string Nrc, string CodigoExperiencia, string NombreExperiencia,
        string? CodigoPlan, string ProgramaEducativo, IReadOnlyList<HorarioPlaneaDTO> Horarios,
        IReadOnlyList<DocentePlaneaDTO> Docentes);

    /// Datos de cabecera de la programación PLANEA de un plan de estudios en un periodo.
    public sealed record EncabezadoProgramacionPlaneaDTO(
        int IdEntidadAcademica, string EntidadAcademica, string? Region,
        string ProgramaEducativo, string? CodigoPlan, string CodigoPeriodo, string PeriodoMostrar = "");

    public sealed record ProgramacionPlaneaDTO(
        UltimaSincronizacionPlaneaDTO? UltimaSincronizacion,
        IReadOnlyList<CopiaProgramacionPlaneaDTO> Copias,
        int Total,
        int Pagina);
}
