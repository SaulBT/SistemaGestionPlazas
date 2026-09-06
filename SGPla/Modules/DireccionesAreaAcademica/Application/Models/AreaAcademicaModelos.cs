namespace SGPla.Modules.DireccionesAreaAcademica.Application.Models;

public sealed record AreaAcademicaRegistro(
    int IdAreaAcademica,
    string Nombre,
    string Telefono,
    string Extension);

public sealed record AreaAcademicaParaCrear(
    string Nombre,
    string Telefono,
    string Extension);

public sealed record AreaAcademicaParaActualizar(
    int IdAreaAcademica,
    string Nombre,
    string Telefono,
    string Extension);

public sealed record AreaAcademicaFiltro(
    string? Busqueda,
    int Pagina,
    int Cantidad);

public sealed record AreasAcademicasPagina(
    IReadOnlyList<AreaAcademicaRegistro> Items,
    int Total);
