using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public class EntidadAcademicaRepository : IEntidadAcademicaRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public EntidadAcademicaRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<List<EntidadAcademica>> ObtenerTodosAsync()
        {
            return await _context.EntidadAcademica
                .AsNoTracking()
                .OrderBy(e => e.Nombre)
                .ToListAsync();
        }

        public async Task<EntidadAcademica?> ObtenerPorIdAsync(int idEntidadAcademica)
        {
            return await _context.EntidadAcademica
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEntidadAcademica == idEntidadAcademica);
        }

        public async Task<List<EntidadAcademica>> ObtenerPorIdAreaAcademicaAsync(int idAreaAcademica)
        {
            if (idAreaAcademica <= 0)
                return new List<EntidadAcademica>();

            return await _context.EntidadAcademica
                .AsNoTracking()
                .Where(e => e.IdAreaAcademica == idAreaAcademica)
                .OrderBy(e => e.Nombre)
                .ToListAsync();
        }

        public async Task<bool> ExistePorIdAsync(int idEntidadAcademica)
        {
            if (idEntidadAcademica <= 0)
                return false;

            return await _context.EntidadAcademica
                .AnyAsync(e => e.IdEntidadAcademica == idEntidadAcademica);
        }

        public async Task<EntidadAcademica> CrearAsync(EntidadAcademica entidadAcademica)
        {
            await _context.EntidadAcademica.AddAsync(entidadAcademica);
            await _context.SaveChangesAsync();
            return entidadAcademica;
        }

        public async Task ActualizarAsync(EntidadAcademica entidadAcademica)
        {
            _context.EntidadAcademica.Update(entidadAcademica);
            await _context.SaveChangesAsync();
        }
    }
}
