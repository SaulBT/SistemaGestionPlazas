using SGPla.Models.DTOs.ProgramacionAcademica;
using SGPla.Models.ViewModels.Catalogos;

namespace SGPla.Repositories.Interfaces;

public interface IProgramacionAcademicaMvcRepository
{
    Task<PaginaProgramacionAcademicaMvc> BuscarAsync(ProgramacionAcademicaMvcFiltro filtro, int? entidadAcademicaAutorizadaId, CancellationToken cancellationToken = default);
    Task<ResultadoImportacionProgramacionMvc> ImportarAsync(int periodoEscolarId, int entidadAcademicaId, IReadOnlyList<ProgramacionAcademicaImportacionFila> filas, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogoOpcion>> ObtenerPeriodosAsync(CancellationToken cancellationToken = default);
}
