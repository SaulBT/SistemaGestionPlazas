using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface IPlanEstudiosRepository
    {
        Task<List<PlanEstudios>> ObtenerTodosAsync();
        Task<List<PlanEstudios>> ObtenerPorPaginaAsync(int pagina, int cantidad);
        Task<int> ContarAsync();
        Task<List<PlanEstudios>> ObtenerPorFiltroAsync(int idEntidadAcademica, int idProgramaEducativo, string? nombre, int pagina, int cantidad);
        Task<int> ContarPorFiltroAsync(int idEntidadAcademica, int idProgramaEducativo, string? nombre);
        Task<PlanEstudios?> ObtenerPorIdAsync(int idPlanEstudios);
        Task<bool> ExistePorIdAsync(int idPlanEstudios);
        Task<bool> ExisteProgramaEducativoPorIdAsync(int idProgramaEducativo);
        Task<PlanEstudios> CrearAsync(PlanEstudios planEstudios);
        Task EliminarAsync(PlanEstudios planEstudios);
        Task EditarAsync(PlanEstudios planEstudios);
    }
}
