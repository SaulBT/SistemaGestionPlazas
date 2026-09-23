using SGPla.Models.DTOs.ProgramaEducativo;

namespace SGPla.Repositories.Interfaces;

public interface IProgramaEducativoMvcRepository
{
    Task<PaginaProgramasEducativosMvc> BuscarAsync(ProgramaEducativoMvcFiltro filtro, CancellationToken cancellationToken = default);
    Task<ProgramaEducativoMvcDto?> ObtenerAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CrearAsync(GuardarProgramaEducativoMvcDto dto, CancellationToken cancellationToken = default);
    Task<bool> ActualizarAsync(GuardarProgramaEducativoMvcDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarAsync(int id, DateTime instanteUtc, CancellationToken cancellationToken = default);
    Task<bool> CatalogosActivosAsync(int entidadAcademicaId, int sistemaEducativoId, int nivelFormacionId, CancellationToken cancellationToken = default);
}
