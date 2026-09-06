namespace SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadesAcademicas.Contracts;

public sealed record ConsultarEntidadesAcademicasQuery(
    string? Busqueda,
    string? Region,
    int? IdAreaAcademica,
    int Pagina,
    int Cantidad);
