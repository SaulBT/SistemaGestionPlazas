namespace SGPla.Modules.ProgramasEducativos.Application.Contracts;

public sealed record ProgramaEducativoResponse(
    int IdProgramaEducativo,
    string Nombre,
    string Campus,
    int IdEntidadAcademica,
    string NombreEntidadAcademica,
    int IdAreaAcademica,
    string NombreAreaAcademica,
    string Region,
    IReadOnlyList<PlanEstudioResponse>? PlanesEstudio = null);

public sealed record ProgramaEducativoListaResponse(
    int IdProgramaEducativo,
    string Nombre,
    string Campus,
    int IdEntidadAcademica,
    string NombreEntidadAcademica,
    int IdAreaAcademica,
    string NombreAreaAcademica,
    string Region,
    IReadOnlyList<PlanEstudioResumenResponse> PlanesEstudio);

public sealed record PlanEstudioResumenResponse(
    int IdPlanEstudios,
    string Nombre);

public sealed record PlanEstudioResponse(
    int IdPlanEstudios,
    string Nombre,
    string? Modalidad,
    int? IdArchivo,
    string? NombreArchivo,
    string? TipoArchivo,
    double? TamanioArchivo,
    int CantidadExperienciasEducativas);

public sealed record ProgramasEducativosResponse(
    IReadOnlyList<ProgramaEducativoListaResponse> Items,
    int Pagina,
    int Cantidad,
    int Total);
