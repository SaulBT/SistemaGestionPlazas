namespace SGPla.Modules.ProgramasEducativos.Application.ActualizarProgramaEducativo.Contracts;

public sealed record ActualizarProgramaEducativoCommand(
    int IdProgramaEducativo,
    string? Nombre,
    string? Campus,
    int? IdEntidadAcademica);
