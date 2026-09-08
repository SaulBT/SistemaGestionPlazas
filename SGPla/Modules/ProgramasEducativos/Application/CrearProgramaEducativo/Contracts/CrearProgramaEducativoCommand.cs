namespace SGPla.Modules.ProgramasEducativos.Application.CrearProgramaEducativo.Contracts;

using SGPla.Modules.ProgramasEducativos.Application.Models;

public sealed record CrearProgramaEducativoCommand(
    string? Nombre,
    string? Campus,
    int? IdEntidadAcademica,
    IReadOnlyList<PlanEstudioParaGuardar> PlanesEstudio);
