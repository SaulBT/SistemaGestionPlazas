namespace SGPla.Modules.DireccionesAreaAcademica.Application.Contracts;

public sealed record AreaAcademicaResponse(
    int IdAreaAcademica,
    string Nombre,
    string Telefono,
    string Extension);

public sealed record AreasAcademicasResponse(
    IReadOnlyList<AreaAcademicaResponse> Items,
    int Pagina,
    int Cantidad,
    int Total);
