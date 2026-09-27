namespace SGPla.Models.DTOs.Integracion;

public readonly record struct PlaneaHorarioDia(byte DiaSemana, string? Inicio, string? Fin);

public sealed record PlaneaRegistro(string? Periodo, string? Nrc, string? NumeroPersonal, string? Nombre,
    string? Edificio, string? Aula, string? FechaInicio, string? FechaFin,
    IReadOnlyList<PlaneaHorarioDia> Horarios, string? IndDocente = null, string? IndPrincipal = null,
    string? Responsabilidad = null);

public sealed record PlaneaSesionValidada(int ProgramacionAcademicaId, byte DiaSemana, TimeOnly HoraInicio,
    TimeOnly HoraFin, DateOnly FechaInicio, DateOnly FechaFin, string? Edificio, string? Aula);

public sealed record PlaneaDocenteValidado(int ProgramacionAcademicaId, string NumeroPersonal, string Nombre,
    DateOnly FechaInicio, DateOnly FechaFin);

public sealed record PlaneaSnapshotValidado(IReadOnlyList<PlaneaSesionValidada> Sesiones,
    IReadOnlyList<PlaneaDocenteValidado> Docentes, int RegistrosIgnorados, int DuplicadosDescartados,
    int Advertencias, int NrcSinProgramacion = 0);

public sealed record ProgramacionPlaneaReferencia(int Id, string Nrc);
