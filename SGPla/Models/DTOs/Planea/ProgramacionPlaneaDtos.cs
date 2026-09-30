namespace SGPla.Models.DTOs.Planea
{
    public sealed class FiltroProgramacionPlaneaDTO
    {
        public int? IdPeriodo { get; set; }
        public int? IdEntidadAcademica { get; set; }
        public int? IdProgramaEducativo { get; set; }
        public string? Busqueda { get; set; }
        public int Limite { get; set; } = 100;
    }

    public sealed record UltimaSincronizacionPlaneaDTO(
        string CodigoPeriodo, string Estado, DateTime FechaInicio, DateTime? FechaFin,
        int? NrcRecibidos, int? NrcNuevos, int? NrcExistentes, int? NrcSinPlan, int? NrcSinExperiencia,
        int? HorariosInsertados, string? MensajeError);

    public sealed record HorarioPlaneaDTO(string Dia, TimeOnly HoraInicio, TimeOnly HoraFin, string? Edificio, string? Aula);

    public sealed record CopiaProgramacionPlaneaDTO(
        string CodigoPeriodo, string Nrc, string CodigoExperiencia, string NombreExperiencia,
        string? CodigoPlan, string ProgramaEducativo, string? Region, IReadOnlyList<HorarioPlaneaDTO> Horarios);

    public sealed record ProgramacionPlaneaDTO(
        UltimaSincronizacionPlaneaDTO? UltimaSincronizacion,
        IReadOnlyList<CopiaProgramacionPlaneaDTO> Copias,
        int Total);
}
