using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface ICoordinadorEaRepository
    {
        Task<int> CrearAsync(CoordinadorEa coordinadorEa);
        Task<CoordinadorEa?> ObtenerPorIdAsync(int idCoordinadorEa);
        Task<List<CoordinadorEa>> ObtenerTodosAsync();
        Task<List<CoordinadorEa>> ObtenerPorFiltrosAsync(string? region, int? idAreaAcademica, int? idEntidadAcademica);
        Task ActualizarAsync(CoordinadorEa coordinadorEa);
        Task EliminarAsync(CoordinadorEa coordinadorEa);
    }
}
