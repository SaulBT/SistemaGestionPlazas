using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Models.ViewModels.Catalogos;

namespace SGPla.Services.Interfaces;

public interface IProgramaEducativoMvcService
{
    Task<PaginaProgramasEducativosMvc> BuscarAsync(ProgramaEducativoMvcFiltro filtro, CancellationToken cancellationToken = default);
    Task<ProgramaEducativoMvcDto?> ObtenerAsync(int id, CancellationToken cancellationToken = default);
    Task<int> GuardarAsync(GuardarProgramaEducativoMvcDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogoOpcion>> ObtenerEntidadesAsync(int? regionId, int? areaAcademicaId, CancellationToken cancellationToken = default);
}
