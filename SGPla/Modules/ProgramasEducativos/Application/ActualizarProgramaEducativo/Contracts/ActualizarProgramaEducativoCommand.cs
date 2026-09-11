namespace SGPla.Modules.ProgramasEducativos.Application.ActualizarProgramaEducativo.Contracts;

using SGPla.Modules.ProgramasEducativos.Application.Models;

public sealed record ActualizarProgramaEducativoCommand(
    int IdProgramaEducativo,
    string? Nombre,
    string? Campus,
    int? IdEntidadAcademica,
    IReadOnlyList<PlanEstudioParaGuardar> PlanesEstudio);
