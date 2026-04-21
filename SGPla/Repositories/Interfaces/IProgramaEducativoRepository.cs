using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface IProgramaEducativoRepository
    {
        Task<List<ProgramaEducativo>> ObtenerTodosAsync();
        Task<ProgramaEducativo?> ObtenerPorIdAsync(int idProgramaEducativo);
        Task<List<ProgramaEducativo>> ObtenerPorFiltrosAsync(string busqueda);
        Task<ProgramaEducativo?> ExisteAsync(ProgramaEducativo programaEducativo);
        Task<ProgramaEducativo> CrearAsync(ProgramaEducativo programaEducativo);
        Task<ProgramaEducativo?> ActualizarAsync(ProgramaEducativo programaEducativo);

        Task<bool> EliminarAsync(int id);
    }
}
