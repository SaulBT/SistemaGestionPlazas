using SGPla.Models.DTOs.Usuarios;

namespace SGPla.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<int> CrearAsync(CrearUsuarioDTO dto);

        Task<List<ListaUsuarioDTO>> ObtenerTodosAsync();

        Task<List<ListaUsuarioDTO>> ObtenerPorFiltroAsync(FiltrosUsuarioDTO filtro);

        Task<DetallesUsuarioDTO?> ObtenerPorIdAsync(int idUsuario);

        Task EditarAsync(ReferenciaUsuarioDTO dto);

        Task EliminarAsync(int idUsuario);
    }
}
