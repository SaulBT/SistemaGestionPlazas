using SGPla.Models;


namespace SGPla.Repositories.Interfaces
{
    public interface IArticuloRepository
    {
        Task<IEnumerable<Articulo>> ObtenerTodosAsync();
        Task<Articulo?> ObtenerArticuloPorIdAsync(int id);

        Task<Articulo> CrearArticuloAsync(Articulo articulo);

        Task ActualizarArticuloAsync(Articulo articulo);

        Task EliminarArticuloAsync(int id);

        Task<bool> ExisteNumeroAsync(string numero);
    }
}
