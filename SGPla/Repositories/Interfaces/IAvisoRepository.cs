using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface IAvisoRepository
    {
        Task<List<Aviso>> ObtenerTodos();
        Task<Aviso?> ObtenerPorID(int idAviso);
        Task<Aviso> CrearAviso(Aviso aviso);
        Task AsociarOfertasPorAviso(List<int> idsOfertas, int idAviso);
        Task EliminarAvisoPorId(int idAviso);
        Task ActualizarAvisoPorId(Aviso aviso);
    }
}
