namespace SGPla.Repositories.Implementations
{
    using Microsoft.EntityFrameworkCore;
    using SGPla.Data;
    using SGPla.Models;
    using SGPla.Repositories.Interfaces;

    public class AreaAcademicaRepository : IAreaAcademicaRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public AreaAcademicaRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<List<AreaAcademica>> ObtenerTodosAsync()
        {
            return await _context.AreaAcademica
                .AsNoTracking()
                .OrderBy(a => a.Nombre)
                .ToListAsync();
        }

        public async Task<AreaAcademica?> ObtenerPorIdAsync(int idAreaAcademica)
        {
            return await _context.AreaAcademica
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAreaAcademica == idAreaAcademica);
        }

        public async Task<bool> ExistePorIdAsync(int idAreaAcademica)
        {
            if (idAreaAcademica <= 0)
                return false;

            return await _context.AreaAcademica
                .AnyAsync(a => a.IdAreaAcademica == idAreaAcademica);
        }

        public async Task<AreaAcademica> CrearAsync(AreaAcademica areaAcademica)
        {
            await _context.AreaAcademica.AddAsync(areaAcademica);
            await _context.SaveChangesAsync();
            return areaAcademica;
        }

        public async Task ActualizarAsync(AreaAcademica areaAcademica)
        {
            _context.AreaAcademica.Update(areaAcademica);
            await _context.SaveChangesAsync();
        }
    }
}
