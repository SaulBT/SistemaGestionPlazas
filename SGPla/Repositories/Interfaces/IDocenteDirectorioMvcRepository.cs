using SGPla.Models.DTOs.Docentes;

namespace SGPla.Repositories.Interfaces;

public interface IDocenteDirectorioMvcRepository
{
    Task<PaginaDocenteDirectorioMvc> BuscarAsync(DocenteDirectorioMvcFiltro filtro, CancellationToken cancellationToken = default);
}
