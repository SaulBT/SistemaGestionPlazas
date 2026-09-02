using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public class CoordinadorEaRepository : ICoordinadorEaRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public CoordinadorEaRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<int> CrearAsync(CoordinadorEa coordinadorEa)
        {
            _context.CoordinadorEa.Add(coordinadorEa);
            await _context.SaveChangesAsync();
            return coordinadorEa.IdCoordinadorEa;
        }

        public async Task<bool> ExisteCorreoAsync(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return false;

            return await _context.CoordinadorEa
                .AnyAsync(coordinadorEa => coordinadorEa.Correo == correo);
        }

        public async Task<CoordinadorEa?> ObtenerPorIdAsync(int idCoordinadorEa)
        {
            return await _context.CoordinadorEa
                .Include(c => c.IdEntidadAcademicaNavigation)
                .ThenInclude(e => e.IdAreaAcademicaNavigation)
                .FirstOrDefaultAsync(c => c.IdCoordinadorEa == idCoordinadorEa);
        }
        public async Task<CoordinadorEa?> ObtenerPorCorreoAsync(string correo)
        {
            return await _context.CoordinadorEa
                .Include(c => c.IdEntidadAcademicaNavigation)
                .ThenInclude(e => e.IdAreaAcademicaNavigation)
                .FirstOrDefaultAsync(c => c.Correo == correo);
        }

        public async Task<List<CoordinadorEa>> ObtenerTodosAsync()
        {
            return await _context.CoordinadorEa
                .Include(c => c.IdEntidadAcademicaNavigation)
                .ThenInclude(e => e.IdAreaAcademicaNavigation)
                .ToListAsync();
        }

        public async Task<List<CoordinadorEa>> BuscarConFiltros(string? region, int? idAreaAcademica, int? idEntidadAcademica, string? busqueda)
        {
            var query = _context.CoordinadorEa
                .Include(c => c.IdEntidadAcademicaNavigation)
                .ThenInclude(e => e.IdAreaAcademicaNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(region))
            {
                query = query.Where(c => c.IdEntidadAcademicaNavigation.Region == region);
            }

            if (idAreaAcademica.HasValue)
            {
                query = query.Where(c => c.IdEntidadAcademicaNavigation.IdAreaAcademica == idAreaAcademica.Value);
            }

            if (idEntidadAcademica.HasValue)
            {
                query = query.Where(c => c.IdEntidadAcademica == idEntidadAcademica.Value);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.ToLower();

                query = query.Where(c =>
                    (c.Nombre ?? "").ToLower().Contains(texto) ||
                    (c.Correo ?? "").ToLower().Contains(texto) ||
                    (c.Cargo ?? "").ToLower().Contains(texto)
                );
            }

            return await query.ToListAsync();
        }

        public async Task ActualizarAsync(CoordinadorEa coordinadorEa)
        {
            _context.CoordinadorEa.Update(coordinadorEa);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(CoordinadorEa coordinadorEa)
        {
            _context.CoordinadorEa.Remove(coordinadorEa);
            await _context.SaveChangesAsync();
        }
    }
}
