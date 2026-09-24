namespace SGPla.Models.DTOs.Integracion;

public sealed record PlaneaHorarioDia(byte DiaSemana, string? Inicio, string? Fin);

public sealed record PlaneaRegistro(string? Periodo, string? Nrc, string? NumeroPersonal, string? Nombre,
    string? Edificio, string? Aula, string? FechaInicio, string? FechaFin,
    IReadOnlyList<PlaneaHorarioDia> Horarios);

public sealed record PlaneaSesionValidada(int ProgramacionAcademicaId, byte DiaSemana, TimeOnly HoraInicio,
    TimeOnly HoraFin, DateOnly FechaInicio, DateOnly FechaFin, string? Edificio, string? Aula);

public sealed record PlaneaDocenteValidado(int ProgramacionAcademicaId, string NumeroPersonal, string Nombre,
    DateOnly FechaInicio, DateOnly FechaFin);

public sealed record PlaneaSnapshotValidado(IReadOnlyList<PlaneaSesionValidada> Sesiones,
    IReadOnlyList<PlaneaDocenteValidado> Docentes, int RegistrosIgnorados, int DuplicadosDescartados,
    int Advertencias);

public sealed record ProgramacionPlaneaReferencia(int Id, string Nrc);
