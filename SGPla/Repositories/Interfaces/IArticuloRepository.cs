using SGPla.Models;


namespace SGPla.Repositories.Interfaces
{
    public interface IArticuloRepository
    {
        Task<IEnumerable<Articulo>> ObtenerTodosAsync();
        Task<Articulo?> ObtenerArticuloPorIdAsync(int id);

        Task<Articulo> CrearArticuloAsync(Articulo articulo);

        Task<Articulo?> ActualizarArticuloAsync(Articulo articulo);

        Task<bool> EliminarArticuloAsync(int id);

        Task<Articulo?> ExisteAsync(string numero);

        Task<IEnumerable<Articulo>> BuscarPorTerminoAsync(string busqueda);


        Task<Articulo?> ObtenerArticuloPorNumero(int numero);

    }
}
