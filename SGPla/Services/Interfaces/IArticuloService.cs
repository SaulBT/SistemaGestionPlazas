using SGPla.Models;
using SGPla.Models.DTOs.Articulo;

namespace SGPla.Services.Interfaces
{
    public interface IArticuloService
    {
        Task<IEnumerable<Articulo>> ObtenerTodosAsync();
        Task<Articulo?> ObtenerArticuloPorIdAsync(int id);

        Task<ArticuloDTO> CrearArticuloAsync(FormularioArticuloDTO articulo);

        Task ActualizarArticuloAsync(Articulo articulo);

        Task EliminarArticuloAsync(int id);
    }
}
