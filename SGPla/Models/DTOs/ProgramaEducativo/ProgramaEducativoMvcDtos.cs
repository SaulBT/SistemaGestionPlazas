namespace SGPla.Models.DTOs.ProgramaEducativo;

public sealed record ProgramaEducativoMvcDto(
    int Id,
    string Nombre,
    int EntidadAcademicaId,
    string EntidadAcademica,
    int AreaAcademicaId,
    string AreaAcademica,
    int CampusId,
    string Campus,
    int RegionId,
    string Region,
    int SistemaEducativoId,
    string SistemaEducativo,
    int NivelFormacionId,
    string NivelFormacion);

public sealed class ProgramaEducativoMvcFiltro
{
    public string? Nombre { get; set; }
    public int? RegionId { get; set; }
    public int? AreaAcademicaId { get; set; }
    public int? EntidadAcademicaId { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 10;
}

public sealed class GuardarProgramaEducativoMvcDto
{
    public int? Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int EntidadAcademicaId { get; set; }
    public int SistemaEducativoId { get; set; }
    public int NivelFormacionId { get; set; }
}

public sealed record PaginaProgramasEducativosMvc(IReadOnlyList<ProgramaEducativoMvcDto> Items, int Total);
