using SGPla.Models;
using SGPla.Models.DTOs.Aviso;

namespace SGPla.Repositories.Interfaces
{
    public interface IAvisoRepository
    {
        Task<List<Aviso>> ObtenerTodosAsync(FiltroAvisosDTO filtro);
        Task<Aviso?> ObtenerPorIDAsync(int idAviso);
        Task CrearAsync(Aviso aviso);
        Task EliminarAsync(int idAviso);
        Task ActualizarAsync(Aviso aviso);
        Task<int> ContarAsync(FiltroAvisosDTO filtro);
    }
}
