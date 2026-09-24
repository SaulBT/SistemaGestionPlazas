namespace SGPla.Models.DTOs.PlanEstudios;

public sealed record PlanEstudiosMvcFiltro
{
    public string? Busqueda { get; init; }
    public int? RegionId { get; init; }
    public int? AreaAcademicaId { get; init; }
    public int? EntidadAcademicaId { get; init; }
    public int? ProgramaEducativoId { get; init; }
    public int Pagina { get; init; } = 1;
    public int TamanoPagina { get; init; } = 10;
}

public sealed record PlanEstudiosMvcFila(
    int Id, string Codigo, int ProgramaEducativoId, string ProgramaEducativo,
    int EntidadAcademicaId, string EntidadAcademica, int AreaAcademicaId, string AreaAcademica,
    int RegionId, string Region, string SistemaEducativo, string NivelFormacion);

public sealed record ExperienciaEducativaMvcDto(
    int Id, string Nombre, string MateriaEe, string CursoEe, int HorasTeoricas,
    int HorasPracticas, int Creditos, string? PerfilDocente, int AreaFormacionId, string AreaFormacion);

public sealed record PlanEstudiosMvcDetalle(
    int Id, string Codigo, int ProgramaEducativoId, string ProgramaEducativo,
    int EntidadAcademicaId, string EntidadAcademica, string AreaAcademica,
    string Region, string SistemaEducativo, string NivelFormacion);

public sealed class GuardarPlanEstudiosMvcDto
{
    public int ProgramaEducativoId { get; init; }
    public string Codigo { get; init; } = string.Empty;
}

public sealed class GuardarExperienciaEducativaMvcDto
{
    public int Id { get; init; }
    public int PlanEstudiosId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string MateriaEe { get; init; } = string.Empty;
    public string CursoEe { get; init; } = string.Empty;
    public int HorasTeoricas { get; init; }
    public int HorasPracticas { get; init; }
    public int Creditos { get; init; }
    public string? PerfilDocente { get; init; }
    public int AreaFormacionId { get; init; }
}

public sealed record PaginaExperienciasEducativasMvc(
    IReadOnlyList<ExperienciaEducativaMvcDto> Items, int Total);

public sealed record PaginaPlanesEstudiosMvc(
    IReadOnlyList<PlanEstudiosMvcFila> Items, int Total);

public sealed record PlanEstudiosExcelImportacionMvc(
    IReadOnlyList<GuardarExperienciaEducativaMvcDto> Experiencias, string? Error);
