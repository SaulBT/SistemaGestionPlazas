using SGPla.Modules.DireccionesAreaAcademica.Application.Models;

namespace SGPla.Modules.DireccionesAreaAcademica.Application.Ports;

public interface IAreaAcademicaRepository
{
    Task<AreasAcademicasPagina> ObtenerPorFiltroAsync(
        AreaAcademicaFiltro filtro,
        CancellationToken cancellationToken);

    Task<AreaAcademicaRegistro?> ObtenerPorIdAsync(
        int idAreaAcademica,
        CancellationToken cancellationToken);

    Task<AreaAcademicaRegistro> CrearAsync(
        AreaAcademicaParaCrear areaAcademica,
        CancellationToken cancellationToken);

    Task<AreaAcademicaRegistro?> ActualizarAsync(
        AreaAcademicaParaActualizar areaAcademica,
        CancellationToken cancellationToken);

    Task<bool> EliminarAsync(
        int idAreaAcademica,
        DateTime fechaEliminacion,
        CancellationToken cancellationToken);
}
