using SGPla.Models.DTOs.Docentes;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class DocenteDirectorioMvcService : IDocenteDirectorioMvcService
{
    private readonly IDocenteDirectorioMvcRepository _repository;

    public DocenteDirectorioMvcService(IDocenteDirectorioMvcRepository repository) => _repository = repository;

    public Task<PaginaDocenteDirectorioMvc> BuscarAsync(DocenteDirectorioMvcFiltro filtro, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        if (filtro.Pagina < 1 || filtro.TamanoPagina is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(filtro), "La página y su tamaño deben estar dentro de los límites permitidos.");
        return _repository.BuscarAsync(filtro, cancellationToken);
    }
}
