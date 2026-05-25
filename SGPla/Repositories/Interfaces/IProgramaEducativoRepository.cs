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

        Task<int> ContarPorFiltroAsync(BuscarProgramaEducativoDTO filtro);


        Task<bool> EliminarAsync(int id);

        Task<bool> EstaAsociadoAPlan(int id);

        Task<List<string>> ObtenerNombresProgramasRegistradosAsync(
        List<string> programas);

    }
}
