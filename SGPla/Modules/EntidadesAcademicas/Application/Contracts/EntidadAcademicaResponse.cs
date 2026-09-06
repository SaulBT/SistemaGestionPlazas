namespace SGPla.Modules.EntidadesAcademicas.Application.Contracts;

public sealed record EntidadAcademicaResponse(
    int IdEntidadAcademica,
    string? Clave,
    string Nombre,
    string CalleNumero,
    string Colonia,
    string Cp,
    string Municipio,
    string Telefono,
    string Extension,
    int IdAreaAcademica,
    string NombreAreaAcademica,
    string Region);

public sealed record EntidadesAcademicasResponse(
    IReadOnlyList<EntidadAcademicaResponse> Items,
    int Pagina,
    int Cantidad,
    int Total);
