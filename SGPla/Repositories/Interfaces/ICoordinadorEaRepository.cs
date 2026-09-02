using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface ICoordinadorEaRepository
    {
        Task<int> CrearAsync(CoordinadorEa coordinadorEa);
        Task<bool> ExisteCorreoAsync(string correo);
        Task<CoordinadorEa?> ObtenerPorIdAsync(int idCoordinadorEa);
        Task<CoordinadorEa?> ObtenerPorCorreoAsync(string correo);
        Task<List<CoordinadorEa>> ObtenerTodosAsync();
        Task<List<CoordinadorEa>> BuscarConFiltros(string? region, int? idAreaAcademica, int? idEntidadAcademica, string? busqueda);
        Task ActualizarAsync(CoordinadorEa coordinadorEa);
        Task EliminarAsync(CoordinadorEa coordinadorEa);
    }
}
