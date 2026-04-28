using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface IPlanEstudiosRepository
    {
        Task<List<PlanEstudios>> ObtenerTodosAsync();
        Task<List<PlanEstudios>> ObtenerDiezAsync(int indiceInicial);
        Task<List<PlanEstudios>> ObtenerPorFiltroAsync(int idEntidadAcademica, int idProgramaEducativo, string? nombre, int indiceInicial);
        Task<PlanEstudios?> ObtenerPorIdAsync(int idPlanEstudios);
        Task<bool> ExistePorIdAsync(int idPlanEstudios);
        Task<bool> ExisteProgramaEducativoPorIdAsync(int idProgramaEducativo);
        Task<PlanEstudios> CrearAsync(PlanEstudios planEstudios);
        Task EliminarAsync(PlanEstudios planEstudios);
        Task EditarAsync(PlanEstudios planEstudios);
    }
}
