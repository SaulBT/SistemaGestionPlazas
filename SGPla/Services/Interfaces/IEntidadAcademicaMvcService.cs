using SGPla.Models.DTOs.EntidadAcademica;

namespace SGPla.Services.Interfaces;

public interface IEntidadAcademicaMvcService
{
    Task<(IReadOnlyList<EntidadAcademicaMvcDto> Items, int TotalCount)> BuscarAsync(
        FiltroEntidadAcademicaMvcDto filtro, CancellationToken cancellationToken = default);
    Task<EntidadAcademicaMvcDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CrearAsync(EntidadAcademicaMvcInputDto datos, CancellationToken cancellationToken = default);
    Task ActualizarAsync(int id, EntidadAcademicaMvcInputDto datos, CancellationToken cancellationToken = default);
    Task EliminarAsync(int id, CancellationToken cancellationToken = default);
}
