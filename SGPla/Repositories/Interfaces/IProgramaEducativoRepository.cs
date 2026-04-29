using SGPla.Models;
using SGPla.Models.DTOs.ProgramaEducativo;

namespace SGPla.Repositories.Interfaces
{
    public interface IProgramaEducativoRepository
    {
        Task<List<ProgramaEducativo>> ObtenerTodosAsync();
        Task<ProgramaEducativo?> ObtenerPorIdAsync(int idProgramaEducativo);
        Task<List<ProgramaEducativo>> ObtenerPorFiltroAsync(BuscarProgramaEducativoDTO filtro);
        Task<ProgramaEducativo?> ExisteAsync(ProgramaEducativo programaEducativo);
        Task<ProgramaEducativo> CrearAsync(ProgramaEducativo programaEducativo);
        Task<ProgramaEducativo?> ActualizarAsync(ProgramaEducativo programaEducativo);

        Task<bool> EliminarAsync(int id);
    }
}
