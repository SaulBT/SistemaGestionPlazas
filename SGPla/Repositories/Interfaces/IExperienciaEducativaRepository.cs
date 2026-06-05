using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface IExperienciaEducativaRepository
    {
        Task<bool> ExisteCodigoExperienciaEducativaEnSistemaAsync(string codigo);
        Task<bool> ExisteExperienciaEducativaPorIdAsync(int idExperienciaEducativa);
        Task<bool> ExperienciaEducativaPerteneceAPlanAsync(int idExperienciaEducativa, int idPlanEstudios);
        Task<bool> ExisteCodigoExperienciaEducativaEnOtroPlanAsync(int idPlanEstudios, string codigo);
        Task<List<ExperienciaEducativa>> ObtenerExperienciasEducativasPorIdPlanEstudiosAsync(int idPlanEstudios);
        Task CrearExperienciasEducativasAsync(List<ExperienciaEducativa> experienciasEducativas);
        Task EliminarExperienciasEducativasPorIdsAsync(List<int> idsExperienciasEducativas);
        Task ActualizarExperienciasEducativasAsync(List<ExperienciaEducativa> experienciasEducativas);

        Task<Dictionary<string, int>> ObtenerIdsPorNombreAsync(List<string> nombres);
    }
}
