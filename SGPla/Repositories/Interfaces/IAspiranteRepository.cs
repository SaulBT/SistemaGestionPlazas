using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface IAspiranteRepository
    {
        Task<Docente> RegistrarAsync(Docente docente);
        Task<Docente?> ObtenerPorIdAsync(int idDocente);
        Task<List<Docente>> ObtenerTodosAsync();
        Task<List<Docente>> ObtenerPorPaginaAsync(string busqueda, int pagina, int cantidad);
        Task EditarAsync(Docente docenteEditado);
        Task EliminarAsync(Docente docente);
        Task<int> ContarAsync(string busqueda);
        Task<bool> ExistePorIdAsync(int idDocente);
    }
}
