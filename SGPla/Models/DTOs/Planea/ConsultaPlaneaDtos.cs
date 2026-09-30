namespace SGPla.Models.DTOs.Planea
{
    public sealed class FiltroBitacoraPlaneaDTO { public int? IdPeriodo { get; set; } public string? Estado { get; set; } public int Pagina { get; set; } = 1; public int Cantidad { get; set; } = 10; }
    public sealed record BitacoraPlaneaDTO(int IdSincronizacion, string CodigoPeriodo, DateTime FechaInicio, DateTime? FechaFin, string Estado, int? RegistrosRecibidos, int? NrcRecibidos, int? NrcNuevos, int? NrcExistentes, int? NrcSinPlan, int? NrcSinExperiencia, int? HorariosInsertados, string? Advertencias, string? MensajeError);
    public sealed class FiltroCopiaPlaneaDTO { public int? IdPeriodo { get; set; } public string? Busqueda { get; set; } public int? IdPlanEstudios { get; set; } public int? IdRegion { get; set; } public int Pagina { get; set; } = 1; public int Cantidad { get; set; } = 20; }
    public sealed record CopiaPlaneaFilaDTO(int IdExperienciaEducativaPeriodo, string CodigoPeriodo, string Nrc, string CodigoExperiencia, string NombreExperiencia, string? CodigoPlan, string? Region, string? Campus, int Horarios, DateTime FechaAlta);
    public sealed record HorarioCopiaDTO(string Dia, TimeOnly HoraInicio, TimeOnly HoraFin, string? Edificio, string? Aula, DateOnly? FechaInicio, DateOnly? FechaFin);
    public sealed record DetalleCopiaPlaneaDTO(CopiaPlaneaFilaDTO Copia, string TituloPlanea, string NombrePlan, string? Nivel, string? Area, int IdSincronizacionPlanea, IReadOnlyList<HorarioCopiaDTO> Horarios);
}
