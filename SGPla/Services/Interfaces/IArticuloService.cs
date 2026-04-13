using SGPla.Models;
using SGPla.Models.DTOs.Articulo;

namespace SGPla.Services.Interfaces
{
    public interface IArticuloService
    {
        Task<IEnumerable<DetallesArticuloDTO>> ObtenerTodosAsync();
        Task<DetallesArticuloDTO?> ObtenerArticuloPorIdAsync(int id);

        Task<DetallesArticuloDTO> CrearArticuloAsync(CrearArticuloDTO articulo);

        Task<DetallesArticuloDTO> EditarArticuloAsync(EditarArticuloDTO articulo);

        Task<bool> EliminarArticuloAsync(int id);
    }
}
