namespace SGPla.Modules.ProgramasEducativos.Application.Contracts;

public sealed record ProgramaEducativoResponse(
    int IdProgramaEducativo,
    string Nombre,
    string Campus,
    int IdEntidadAcademica,
    string NombreEntidadAcademica,
    int IdAreaAcademica,
    string NombreAreaAcademica,
    string Region);

public sealed record ProgramasEducativosResponse(
    IReadOnlyList<ProgramaEducativoResponse> Items,
    int Pagina,
    int Cantidad,
    int Total);
