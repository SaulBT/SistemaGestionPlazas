using SGPla.Models.DTOs.Usuarios;

namespace SGPla.Repositories.Interfaces;

public interface IUsuarioConsultaRepository
{
    Task<(List<ListaUsuarioDTO> Items, int TotalCount)> BuscarPaginadoAsync(
        FiltrosUsuarioDTO filtro,
        CancellationToken cancellationToken = default);
}
