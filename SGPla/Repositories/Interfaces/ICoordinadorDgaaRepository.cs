using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface ICoordinadorDgaaRepository
    {
        Task<int> CrearAsync(CoordinadorDgaa coordinadorDgaa);
        Task<CoordinadorDgaa?> ObtenerPorIdAsync(int idCoordinadorDgaa);
        Task<List<CoordinadorDgaa>> ObtenerTodosAsync();
        Task<List<CoordinadorDgaa>> ObtenerPorFiltrosAsync(int? idAreaAcademica);
        Task ActualizarAsync(CoordinadorDgaa coordinadorDgaa);
        Task EliminarAsync(CoordinadorDgaa coordinadorDgaa);
    }
}
