namespace SGPla.Modules.EntidadesAcademicas.Application.Models;

public sealed record EntidadAcademicaRegistro(
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

public sealed record EntidadAcademicaParaCrear(
    string Clave,
    string Nombre,
    string CalleNumero,
    string Colonia,
    string Cp,
    string Municipio,
    string Telefono,
    string Extension,
    int IdAreaAcademica,
    string Region);

public sealed record EntidadAcademicaParaActualizar(
    int IdEntidadAcademica,
    string Clave,
    string Nombre,
    string CalleNumero,
    string Colonia,
    string Cp,
    string Municipio,
    string Telefono,
    string Extension,
    int IdAreaAcademica,
    string Region);

public sealed record EntidadAcademicaFiltro(
    string? Busqueda,
    string? Region,
    int? IdAreaAcademica,
    int Pagina,
    int Cantidad);

public sealed record EntidadesAcademicasPagina(
    IReadOnlyList<EntidadAcademicaRegistro> Items,
    int Total);
