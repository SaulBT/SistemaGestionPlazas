using SGPla.Models.DTOs.EntidadAcademica;

namespace SGPla.Repositories.Interfaces;

public interface IEntidadAcademicaMvcRepository
{
    Task<(IReadOnlyList<EntidadAcademicaMvcDto> Items, int TotalCount)> BuscarAsync(
        FiltroEntidadAcademicaMvcDto filtro, CancellationToken cancellationToken = default);
    Task<EntidadAcademicaMvcDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ReferenciasActivasAsync(EntidadAcademicaMvcInputDto datos, CancellationToken cancellationToken = default);
    Task<bool> TieneProgramasAsync(int entidadId, CancellationToken cancellationToken = default);
    Task<bool> TieneUsuariosActivosAsync(int entidadId, CancellationToken cancellationToken = default);
    Task<int> CrearAsync(EntidadAcademicaMvcInputDto datos, CancellationToken cancellationToken = default);
    Task ActualizarAsync(int id, EntidadAcademicaMvcInputDto datos, CancellationToken cancellationToken = default);
    Task EliminarEnCascadaAsync(int id, DateTime fechaEliminacionUtc, CancellationToken cancellationToken = default);
}
