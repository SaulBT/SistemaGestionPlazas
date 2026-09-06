namespace SGPla.Modules.ProgramasEducativos.Application.Models;

public sealed record ProgramaEducativoRegistro(
    int IdProgramaEducativo,
    string Nombre,
    string Campus,
    int IdEntidadAcademica,
    string NombreEntidadAcademica,
    int IdAreaAcademica,
    string NombreAreaAcademica,
    string Region);

public sealed record ProgramaEducativoParaCrear(
    string Nombre,
    string Campus,
    int IdEntidadAcademica);

public sealed record ProgramaEducativoParaActualizar(
    int IdProgramaEducativo,
    string Nombre,
    string Campus,
    int IdEntidadAcademica);

public sealed record ProgramaEducativoFiltro(
    string? Busqueda,
    string? Region,
    int? IdAreaAcademica,
    int? IdEntidadAcademica,
    int Pagina,
    int Cantidad);

public sealed record ProgramasEducativosPagina(
    IReadOnlyList<ProgramaEducativoRegistro> Items,
    int Total);
