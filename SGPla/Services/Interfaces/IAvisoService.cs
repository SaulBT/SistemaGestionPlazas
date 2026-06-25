using SGPla.Models;
using SGPla.Models.DTOs.Aviso;

namespace SGPla.Services.Interfaces
{
    public interface IAvisoService
    {
        Task<List<DatosAvisoDTO>> ObtenerTodos();
        Task<DatosAvisoDTO?> ObtenerPorID(int idAviso);
        Task CrearAviso(CrearAvisoDTO aviso);
        Task EliminarAvisoPorId(int idAviso);
        Task ActualizarAvisoPorId(EditarAvisoDTO aviso);
    }
}
