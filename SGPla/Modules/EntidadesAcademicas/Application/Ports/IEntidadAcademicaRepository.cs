using SGPla.Modules.EntidadesAcademicas.Application.Models;

namespace SGPla.Modules.EntidadesAcademicas.Application.Ports;

public interface IEntidadAcademicaRepository
{
    Task<EntidadesAcademicasPagina> ObtenerPorFiltroAsync(
        EntidadAcademicaFiltro filtro,
        CancellationToken cancellationToken);

    Task<EntidadAcademicaRegistro?> ObtenerPorIdAsync(
        int idEntidadAcademica,
        CancellationToken cancellationToken);

    Task<bool> ExisteAreaAcademicaActivaAsync(
        int idAreaAcademica,
        CancellationToken cancellationToken);

    Task<bool> ExistePorClaveAsync(
        string clave,
        int? idEntidadAcademicaExcluida,
        CancellationToken cancellationToken);

    Task<EntidadAcademicaRegistro> CrearAsync(
        EntidadAcademicaParaCrear entidadAcademica,
        CancellationToken cancellationToken);

    Task<EntidadAcademicaRegistro?> ActualizarAsync(
        EntidadAcademicaParaActualizar entidadAcademica,
        CancellationToken cancellationToken);

    Task<bool> EliminarAsync(
        int idEntidadAcademica,
        DateTime fechaEliminacion,
        CancellationToken cancellationToken);
}
