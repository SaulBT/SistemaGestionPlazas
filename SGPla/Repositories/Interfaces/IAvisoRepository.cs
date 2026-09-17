using SGPla.Models;
using SGPla.Models.DTOs.Aviso;

namespace SGPla.Repositories.Interfaces
{
    public interface IAvisoRepository
    {
        Task<List<Aviso>> ObtenerTodosAsync(FiltroAvisosDTO filtro);
        Task<Aviso?> ObtenerPorIDAsync(int idAviso);
        Task EditarComentariosRevisionAsync(int idAviso, string comentarios);
        Task<int> ContarAsync(FiltroAvisosDTO filtro);
        Task CambiarStatusArchivadoAsync(int idAviso, bool archivado);
        Task CambiarEstadoRevisionAsync(int idAviso, string estado, string comentarios);
        Task EnviarARevisionAsync(int idAviso, string comentarios);
        Task FirmarAsync(int idAviso, int idArchivoFirmado);
        Task<string> VerComentariosAsync(int idAviso);
        Task PublicarAsync(int idAviso, string url);
        Task<bool> VerificarEstadoAsync(int idAviso, string estado);
        Task<bool> ExistePorId(int idAviso);
        Task CrearCompletoAsync(Aviso aviso, List<int> idsOfertas, List<Horario> horarios);
        Task ActualizarCompletoAsync(EditarAvisoDTO aviso, int idArchivoOriginal, List<Horario> horarios);
        Task<(int? idArchivoOriginal, int? idArchivoFirmado)> EliminarCompletoAsync(int idAviso);
    }
}
