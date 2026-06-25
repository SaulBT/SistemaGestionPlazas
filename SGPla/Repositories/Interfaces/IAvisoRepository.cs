using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface IAvisoRepository
    {
        Task<List<Aviso>> ObtenerTodos();
        Task<Aviso?> ObtenerPorID(int idAviso);
        Task CrearAviso(Aviso aviso);
        Task EliminarAvisoPorId(int idAviso);
        Task ActualizarAvisoPorId(Aviso aviso);
    }
}
