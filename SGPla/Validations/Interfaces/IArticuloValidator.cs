using SGPla.Models.DTOs.Articulo;

namespace SGPla.Validations.Interfaces
{
    public interface IArticuloValidator
    {
        Task ValidarCreacionAsync(CrearArticuloDTO crearArticuloDTO);

        Task ValidarEdicionAsync(EditarArticuloDTO editarArticuloDTO);

        Task ValidarBusquedaPorTerminoAsync(string busqueda);

        Task ValidarObtenerPorIdAsync(int id);

        Task ValidarEliminarAsync(int id);
    }
}
