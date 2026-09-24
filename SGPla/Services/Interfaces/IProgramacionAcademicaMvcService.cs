using Microsoft.AspNetCore.Http;
using SGPla.Models.DTOs.ProgramacionAcademica;
using SGPla.Models.ViewModels.Catalogos;

namespace SGPla.Services.Interfaces;

public interface IProgramacionAcademicaMvcService
{
    Task<PaginaProgramacionAcademicaMvc> BuscarAsync(ProgramacionAcademicaMvcFiltro filtro, int? entidadAcademicaAutorizadaId, CancellationToken cancellationToken = default);
    Task<ResultadoImportacionProgramacionMvc> ImportarVacantesYDescargasAsync(int periodoEscolarId, int entidadAcademicaId, IFormFile archivoVacantes, IFormFile archivoDescargas, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogoOpcion>> ObtenerPeriodosAsync(CancellationToken cancellationToken = default);
}
