using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public class PlanEstudiosRepository : IPlanEstudiosRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public PlanEstudiosRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<List<PlanEstudios>> ObtenerTodosAsync()
        {
            return await _context.PlanEstudios
                .AsNoTracking()
                .Include(planEstudios => planEstudios.IdProgramaEducativoNavigation)
                    .ThenInclude(programaEducativo => programaEducativo.IdEntidadAcademicaNavigation)
                    .ThenInclude(entidadAcademica => entidadAcademica.IdAreaAcademicaNavigation)
                .OrderBy(planEstudios => planEstudios.Nombre)
                .ToListAsync();
        }

        public async Task<List<PlanEstudios>> ObtenerDiezAsync(int indiceInicial)
        {
            return await _context.PlanEstudios
                .AsNoTracking()
                .Include(planEstudios => planEstudios.IdProgramaEducativoNavigation)
                    .ThenInclude(programaEducativo => programaEducativo.IdEntidadAcademicaNavigation)
                    .ThenInclude(entidadAcademica => entidadAcademica.IdAreaAcademicaNavigation)
                .OrderBy(planEstudios => planEstudios.Nombre)
                .Skip(indiceInicial - 1)
                .Take(10)
                .ToListAsync();
        }

        public async Task<List<PlanEstudios>> ObtenerPorFiltroAsync(int idEntidadAcademica, int idProgramaEducativo, string? nombre, int indiceInicial)
        {
            var lista = _context.PlanEstudios
                .AsNoTracking()
                .Include(planEstudios => planEstudios.IdProgramaEducativoNavigation)
                    .ThenInclude(programaEducativo => programaEducativo.IdEntidadAcademicaNavigation)
                    .ThenInclude(entidadAcademica => entidadAcademica.IdAreaAcademicaNavigation)
                .AsQueryable();

            if (idEntidadAcademica > 0)
                lista = lista.Where(planEstudios => planEstudios.IdProgramaEducativoNavigation.IdEntidadAcademica == idEntidadAcademica);
            if (idProgramaEducativo > 0)
                lista = lista.Where(planEstudios => planEstudios.IdProgramaEducativo == idProgramaEducativo);
            if (!string.IsNullOrWhiteSpace(nombre))
                lista = lista.Where(planEstudios => planEstudios.Nombre.Contains(nombre.Trim()));

            return await lista
                .OrderBy(planEstudios => planEstudios.Nombre)
                .Skip(indiceInicial - 1)
                .Take(10)
                .ToListAsync();
        }

        public async Task<PlanEstudios?> ObtenerPorIdAsync(int idPlanEstudios)
        {
            return await _context.PlanEstudios
                .AsNoTracking()
                .Include(planEstudios => planEstudios.IdProgramaEducativoNavigation)
                    .ThenInclude(programaEducativo => programaEducativo.IdEntidadAcademicaNavigation)
                    .ThenInclude(entidadAcademica => entidadAcademica.IdAreaAcademicaNavigation)
                .FirstOrDefaultAsync(planEstudios => planEstudios.IdPlanEstudios == idPlanEstudios);
        }

        public async Task<bool> ExistePorIdAsync(int idPlanEstudios)
        {
            if (idPlanEstudios <= 0)
                return false;

            return await _context.PlanEstudios
                .AnyAsync(planEstudios => planEstudios.IdPlanEstudios == idPlanEstudios);
        }

        public async Task<bool> ExisteProgramaEducativoPorIdAsync(int idProgramaEducativo)
        {
            if (idProgramaEducativo <= 0)
                return false;

            return await _context.ProgramaEducativo
                .AnyAsync(programaEducativo => programaEducativo.IdProgramaEducativo == idProgramaEducativo);
        }

        public async Task<PlanEstudios> CrearAsync(PlanEstudios planEstudios)
        {
            await _context.PlanEstudios.AddAsync(planEstudios);
            await _context.SaveChangesAsync();
            return planEstudios;
        }

        public async Task EliminarAsync(PlanEstudios planEstudios)
        {
            ArgumentNullException.ThrowIfNull(planEstudios);

            _context.PlanEstudios.Remove(planEstudios);
            await _context.SaveChangesAsync();
        }

        public async Task EditarAsync(PlanEstudios planEstudios)
        {
            var plan = await _context.PlanEstudios.FindAsync(planEstudios.IdPlanEstudios);
            if (plan is null)
                return;

            plan.IdArchivoPlan = planEstudios.IdArchivoPlan;
            await _context.SaveChangesAsync();
        }
    }
}
