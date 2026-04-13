using SGPla.Models.DTOs.Articulo;

namespace SGPla.Validations.Interfaces
{
    public interface IArticuloValidator
    {
        Task<bool> ValidarCreacionAsync(FormularioArticuloDTO crearArticuloDTO);
    }
}
