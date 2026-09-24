using SGPla.Models.DTOs.PlanEstudios;

namespace SGPla.Repositories.Interfaces;

public interface IPlanEstudiosMvcRepository
{
    Task<PaginaPlanesEstudiosMvc> BuscarAsync(PlanEstudiosMvcFiltro filtro, CancellationToken cancellationToken = default);
    Task<PlanEstudiosMvcDetalle?> ObtenerAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CrearAsync(GuardarPlanEstudiosMvcDto dto, CancellationToken cancellationToken = default);
    Task<bool> ProgramaActivoAsync(int programaEducativoId, CancellationToken cancellationToken = default);
    Task<bool> EliminarAsync(int id, DateTime instanteUtc, CancellationToken cancellationToken = default);
    Task<PaginaExperienciasEducativasMvc> BuscarExperienciasAsync(int planId, string? busqueda, int pagina, int tamanoPagina, CancellationToken cancellationToken = default);
    Task<ExperienciaEducativaMvcDto?> ObtenerExperienciaAsync(int planId, int id, CancellationToken cancellationToken = default);
    Task<int> CrearExperienciaAsync(GuardarExperienciaEducativaMvcDto dto, CancellationToken cancellationToken = default);
    Task<int> CrearExperienciasAsync(IReadOnlyList<GuardarExperienciaEducativaMvcDto> experiencias, CancellationToken cancellationToken = default);
    Task<bool> ActualizarExperienciaAsync(GuardarExperienciaEducativaMvcDto dto, CancellationToken cancellationToken = default);
    Task<bool> TieneProgramacionAsync(int experienciaEducativaId, CancellationToken cancellationToken = default);
    Task<bool> AreaFormacionActivaAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> EliminarExperienciaAsync(int planId, int id, DateTime instanteUtc, CancellationToken cancellationToken = default);
}
