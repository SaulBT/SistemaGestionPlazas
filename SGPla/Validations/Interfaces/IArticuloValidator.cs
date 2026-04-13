using SGPla.Models.DTOs.Articulo;

namespace SGPla.Validations.Interfaces
{
    public interface IArticuloValidator
    {
        Task<bool> ValidarCreacionAsync(CrearArticuloDTO crearArticuloDTO);

        Task<bool> ValidarEdicionAsync(EditarArticuloDTO editarArticuloDTO);
    }
}
