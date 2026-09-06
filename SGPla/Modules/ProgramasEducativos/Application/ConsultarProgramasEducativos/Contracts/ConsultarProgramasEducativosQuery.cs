namespace SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramasEducativos.Contracts;

public sealed record ConsultarProgramasEducativosQuery(
    string? Busqueda,
    string? Region,
    int? IdAreaAcademica,
    int? IdEntidadAcademica,
    int Pagina,
    int Cantidad);
