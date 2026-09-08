using SGPla.Modules.ProgramasEducativos.Application.Models;

namespace SGPla.Modules.ProgramasEducativos.Application.Ports;

public interface IProgramaEducativoRepository
{
    Task<ProgramasEducativosPagina> ObtenerPorFiltroAsync(
        ProgramaEducativoFiltro filtro,
        CancellationToken cancellationToken);

    Task<ProgramaEducativoRegistro?> ObtenerPorIdAsync(
        int idProgramaEducativo,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PlanEstudioRegistro>> ObtenerPlanesEstudioAsync(
        int idProgramaEducativo,
        CancellationToken cancellationToken);

    Task<bool> ExisteEntidadAcademicaActivaAsync(
        int idEntidadAcademica,
        CancellationToken cancellationToken);

    Task<bool> ExistePorClaveAsync(
        string clave,
        int? idProgramaEducativoExcluido,
        CancellationToken cancellationToken);

    Task<ProgramaEducativoRegistro> CrearAsync(
        ProgramaEducativoParaCrear programaEducativo,
        CancellationToken cancellationToken);

    Task<ProgramaEducativoRegistro> CrearConPlanesEstudioAsync(
        ProgramaEducativoParaCrear programaEducativo,
        IReadOnlyList<PlanEstudioParaPersistir> planesEstudio,
        CancellationToken cancellationToken);

    Task<ProgramaEducativoRegistro?> ActualizarAsync(
        ProgramaEducativoParaActualizar programaEducativo,
        CancellationToken cancellationToken);

    Task<ProgramaActualizacionConPlanesResultado?> ActualizarConPlanesEstudioAsync(
        ProgramaEducativoParaActualizar programaEducativo,
        IReadOnlyList<PlanEstudioParaPersistir> planesEstudio,
        CancellationToken cancellationToken);

    Task<bool> PlanesEstudioTienenDependenciasAsync(
        IReadOnlyCollection<int> idsPlanesEstudio,
        CancellationToken cancellationToken);

    Task<bool> EliminarAsync(
        int idProgramaEducativo,
        DateTime fechaEliminacion,
        CancellationToken cancellationToken);
}
