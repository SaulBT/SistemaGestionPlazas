using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface ICoordinadorDgaaRepository
    {
        Task<int> CrearAsync(CoordinadorDgaa coordinadorDgaa);
        Task<bool> ExisteCorreoAsync(string correo);
        Task<CoordinadorDgaa?> ObtenerPorIdAsync(int idCoordinadorDgaa);
        Task<List<CoordinadorDgaa>> ObtenerTodosAsync();
        Task<List<CoordinadorDgaa>> BuscarConFiltros(int? idAreaAcademica, string? busqueda);
        Task ActualizarAsync(CoordinadorDgaa coordinadorDgaa);
        Task EliminarAsync(CoordinadorDgaa coordinadorDgaa);
    }
}
