using SGPla.Modules.PeriodosEscolares.Application.Models;

namespace SGPla.Modules.PeriodosEscolares.Application.Ports;

public interface IPeriodoEscolarRepository
{
    Task<PeriodosEscolaresPagina> ObtenerPorFiltroAsync(
        PeriodoEscolarFiltro filtro,
        CancellationToken cancellationToken);

    Task<PeriodoEscolarRegistro?> ObtenerPorIdAsync(
        int idPeriodoEscolar,
        CancellationToken cancellationToken);

    Task<bool> ExistePorCodigoAsync(
        string codigo,
        int? idPeriodoEscolarExcluido,
        CancellationToken cancellationToken);

    Task<PeriodoEscolarRegistro> CrearAsync(
        PeriodoEscolarParaCrear periodo,
        CancellationToken cancellationToken);

    Task<PeriodoEscolarRegistro?> ActualizarAsync(
        PeriodoEscolarParaActualizar periodo,
        CancellationToken cancellationToken);

    Task<bool> TieneRelacionesAsync(
        int idPeriodoEscolar,
        CancellationToken cancellationToken);

    Task<bool> EliminarAsync(
        int idPeriodoEscolar,
        CancellationToken cancellationToken);
}
