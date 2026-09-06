namespace SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreasAcademicas.Contracts;

public sealed record ConsultarAreasAcademicasQuery(
    string? Busqueda,
    int Pagina,
    int Cantidad);
