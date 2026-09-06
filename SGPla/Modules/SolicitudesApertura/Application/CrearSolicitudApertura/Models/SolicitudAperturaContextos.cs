namespace SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Models;

public sealed record CoordinadorEaContexto(int IdEntidadAcademica);

public sealed record ExperienciaEducativaContexto(
    int IdExperienciaEducativa,
    string Nombre,
    int IdPlanEstudios,
    int IdProgramaEducativo,
    int IdEntidadAcademica,
    int? CantidadMinimaSolicitantes,
    int? CantidadMaximaSolicitantes);

public sealed record ModalidadContexto(int IdModalidad, string Nombre);

public sealed record PeriodoContexto(
    int IdPeriodo,
    string Codigo,
    DateOnly FechaInicio,
    DateOnly FechaFin);

public sealed record SolicitudAperturaDuplicidadContexto(
    int IdExperienciaEducativa,
    string Seccion,
    int IdPeriodo,
    int IdEntidadAcademica,
    int IdProgramaEducativo,
    int IdPlanEstudios);

public sealed record SolicitudAperturaParaCrear(
    int IdExperienciaEducativa,
    int IdPeriodo,
    int IdEntidadAcademica,
    int IdProgramaEducativo,
    int IdPlanEstudios,
    int IdModalidad,
    string Seccion,
    int CantidadSolicitantes,
    string? Justificacion,
    string Estado);
