using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public class ExperienciaEducativaRepository : IExperienciaEducativaRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public ExperienciaEducativaRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExisteCodigoExperienciaEducativaEnSistemaAsync(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return false;

            codigo = codigo.Trim();

            return await _context.ExperienciaEducativa.AnyAsync(experienciaEducativa => experienciaEducativa.Codigo == codigo);
        }

        public async Task<bool> ExisteExperienciaEducativaPorIdAsync(int idExperienciaEducativa)
        {
            if (idExperienciaEducativa <= 0)
                return false;

            return await _context.ExperienciaEducativa
                .AnyAsync(experienciaEducativa => experienciaEducativa.IdExperienciaEducativa == idExperienciaEducativa);
        }

        public async Task<bool> ExperienciaEducativaPerteneceAPlanAsync(int idExperienciaEducativa, int idPlanEstudios)
        {
            return await _context.ExperienciaEducativa
                .AnyAsync(experienciaEducativa => experienciaEducativa.IdExperienciaEducativa == idExperienciaEducativa
                    && experienciaEducativa.IdPlanEstudios == idPlanEstudios);
        }

        public async Task<bool> ExisteCodigoExperienciaEducativaEnOtroPlanAsync(int idPlanEstudios, string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return false;

            codigo = codigo.Trim();

            return await _context.ExperienciaEducativa
                .AnyAsync(experienciaEducativa => experienciaEducativa.Codigo == codigo
                    && experienciaEducativa.IdPlanEstudios != idPlanEstudios);
        }

        public async Task<List<ExperienciaEducativa>> ObtenerExperienciasEducativasPorIdPlanEstudiosAsync(int idPlanEstudios)
        {
            return await _context.ExperienciaEducativa
                .AsNoTracking()
                .Where(experienciaEducativa => experienciaEducativa.IdPlanEstudios == idPlanEstudios)
                .OrderBy(experienciaEducativa => experienciaEducativa.Codigo)
                .ToListAsync();
        }

        public async Task CrearExperienciasEducativasAsync(List<ExperienciaEducativa> experienciasEducativas)
        {
            if (experienciasEducativas.Count == 0)
                return;

            await _context.ExperienciaEducativa.AddRangeAsync(experienciasEducativas);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarExperienciasEducativasPorIdsAsync(List<int> idsExperienciasEducativas)
        {
            if (idsExperienciasEducativas.Count == 0)
                return;

            var experienciasEducativas = await _context.ExperienciaEducativa
                .Where(experienciaEducativa => idsExperienciasEducativas.Contains(experienciaEducativa.IdExperienciaEducativa))
                .ToListAsync();

            _context.ExperienciaEducativa.RemoveRange(experienciasEducativas);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarExperienciasEducativasAsync(List<ExperienciaEducativa> experienciasEducativas)
        {
            if (experienciasEducativas.Count == 0)
                return;

            _context.ExperienciaEducativa.UpdateRange(experienciasEducativas);
            await _context.SaveChangesAsync();
        }
    }
}
