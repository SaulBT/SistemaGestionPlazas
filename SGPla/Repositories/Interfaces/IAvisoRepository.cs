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
        Task CambiarStatusArchivadoAsync(int idAviso, bool archivado);
        Task EnviarARevisionAsync(int idAviso, string comentarios);
        Task FirmarAsync(int idAviso, int idArchivoFirmado);
        Task<string> VerComentariosAsync(int idAviso);
        Task PublicarAsync(int idAviso, string url);
        Task<bool> VerificarEstadoAsync(int idAviso, string estado);
        Task<bool> ExistePorId(int idAviso);
        Task<List<Aviso>> ObtenerTodos();
        Task<Aviso?> ObtenerPorID(int idAviso);
        Task<Aviso> CrearAviso(Aviso aviso);
        Task AsociarOfertasPorAviso(List<int> idsOfertas, int idAviso);
        Task EliminarAvisoPorId(int idAviso);
        Task ActualizarAvisoPorId(Aviso aviso);
    }
}
