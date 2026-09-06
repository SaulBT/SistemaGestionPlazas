namespace SGPla.Modules.ProgramasEducativos.Application.CrearProgramaEducativo.Contracts;

public sealed record CrearProgramaEducativoCommand(
    string? Nombre,
    string? Campus,
    int? IdEntidadAcademica);
