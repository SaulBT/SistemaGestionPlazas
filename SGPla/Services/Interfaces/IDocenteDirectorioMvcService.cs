using SGPla.Models.DTOs.Docentes;

namespace SGPla.Services.Interfaces;

public interface IDocenteDirectorioMvcService
{
    Task<PaginaDocenteDirectorioMvc> BuscarAsync(DocenteDirectorioMvcFiltro filtro, CancellationToken cancellationToken = default);
}
