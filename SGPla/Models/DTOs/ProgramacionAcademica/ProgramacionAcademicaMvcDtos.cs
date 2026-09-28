namespace SGPla.Models.DTOs.ProgramacionAcademica;

public sealed record ProgramacionAcademicaMvcFiltro(
    int? RegionId = null, int? EntidadAcademicaId = null, int? ProgramaEducativoId = null,
    int? PeriodoEscolarId = null, string? Busqueda = null, int Pagina = 1, int TamanoPagina = 25);

public sealed record ProgramacionAcademicaMvcFila(
    int Id, string Nrc, int PeriodoEscolarId, string Periodo, int EntidadAcademicaId,
    string EntidadAcademica, int ProgramaEducativoId, string ProgramaEducativo,
    int ExperienciaEducativaId, string ExperienciaEducativa, string MateriaEe, string CursoEe,
    DateTime? FechaEliminacion);

public sealed record PaginaProgramacionAcademicaMvc(
    IReadOnlyList<ProgramacionAcademicaMvcFila> Items, int Total);

public sealed record ProgramacionAcademicaImportacionFila(string Programa, string ExperienciaEducativa, string Nrc);

public sealed record ResultadoImportacionProgramacionMvc(int Creadas, int YaExistentes, int TotalArchivo);
