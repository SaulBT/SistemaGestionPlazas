namespace SGPla.Models.DTOs.Planea
{
    public sealed record PeriodoPorSincronizar(int IdPeriodo, string Codigo);
    public sealed record CopiaPlanea(string Nrc, string CodigoExperiencia, string CodigoPlan, string Titulo, string? Campus, string? Nivel, string? Region, string? Area);
    public sealed record DocenteCopiaPlanea(string Nrc, string? NumeroPersonal, string Nombre, bool? Imparte);
    public sealed record HorarioCopiaPlanea(int IdHorario, string Nrc, string Dia, TimeOnly HoraInicio, TimeOnly HoraFin, string? Edificio, string? Aula, DateOnly? FechaInicio, DateOnly? FechaFin);
    public sealed record DatosPeriodoPlanea(int NrcRecibidos, int NrcSinPlan, IReadOnlyList<CopiaPlanea> Copias, IReadOnlyList<HorarioCopiaPlanea> Horarios, IReadOnlyList<DocenteCopiaPlanea> Docentes, IReadOnlyList<string> Advertencias);
}
