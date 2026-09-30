using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface IGradoRepository
    {
        Task AgregarAsync(Grado grado);
        Task<Grado?> ObtenerAsync(int idGrad);
        Task<List<Grado>> ObtenerTodosAsync(int idDocente);
        Task EditarAsync(Grado gradoEditado);
        Task EliminarAsync(Grado grado);
        Task<bool> ExsiteAsync(int idGrado);
    }
}
