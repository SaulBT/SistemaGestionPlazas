using SGPla.Models;

namespace SGPla.Repositories
{
    public interface IArticuloRepository
    {
        Task<IEnumerable<Articulo>> ObtenerTodosAsync();
        Task<Articulo?> ObtenerArticuloPorIdAsync(int id);

        Task CrearArticuloAsync(Articulo articulo);

        Task ActualizarArticuloAsync(Articulo articulo);

        Task EliminarArticuloAsync(int id);
    }
}
