using SGPla.Models.DTOs.Usuarios;

namespace SGPla.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<int> CrearAsync(CrearUsuarioDTO dto);

        Task<List<ListaUsuarioDTO>> ObtenerTodosAsync();

        Task<List<ListaUsuarioDTO>> BuscarConFiltrosAsync(FiltrosUsuarioDTO filtro);

        Task<List<ListaUsuarioDTO>> ObtenerPorFiltroAsync(FiltrosUsuarioDTO filtro);

        Task<DetallesUsuarioDTO?> ObtenerPorIdAsync(ReferenciaUsuarioDTO dto);

        Task EditarAsync(EditarUsuarioDTO dto);

        Task EliminarAsync(ReferenciaUsuarioDTO dto);

        Task<(List<ListaUsuarioDTO> Items, int TotalCount)> BuscarPorFiltroPaginadoAsync(FiltrosUsuarioDTO filtro);
    }
}
