using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public class CoordinadorDgaaRepository : ICoordinadorDgaaRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public CoordinadorDgaaRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<int> CrearAsync(CoordinadorDgaa coordinadorDgaa)
        {
            _context.CoordinadorDgaa.Add(coordinadorDgaa);
            await _context.SaveChangesAsync();
            return coordinadorDgaa.IdCoordinadorDgaa;
        }

        public async Task<bool> ExisteCorreoAsync(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return false;

            return await _context.CoordinadorDgaa
                .AnyAsync(coordinadorDgaa => coordinadorDgaa.Correo == correo);
        }

        public async Task<CoordinadorDgaa?> ObtenerPorIdAsync(int idCoordinadorDgaa)
        {
            return await _context.CoordinadorDgaa
                .Include(c => c.IdAreaAcademicaNavigation)
                .FirstOrDefaultAsync(c => c.IdCoordinadorDgaa == idCoordinadorDgaa);
        }

        public async Task<CoordinadorDgaa?> ObtenerPorCorreoAsync(string correo)
        {
            return await _context.CoordinadorDgaa
                .Include(c => c.IdAreaAcademicaNavigation)
                .FirstOrDefaultAsync(c => c.Correo == correo);
        }

        public async Task<List<CoordinadorDgaa>> ObtenerTodosAsync()
        {
            return await _context.CoordinadorDgaa
                .Include(c => c.IdAreaAcademicaNavigation)
                .ToListAsync();
        }

        public async Task<List<CoordinadorDgaa>> BuscarConFiltros(int? idAreaAcademica, string? busqueda)
        {
            var query = _context.CoordinadorDgaa
                .Include(c => c.IdAreaAcademicaNavigation)
                .AsQueryable();

            if (idAreaAcademica.HasValue)
            {
                query = query.Where(c => c.IdAreaAcademica == idAreaAcademica.Value);
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

        public async Task ActualizarAsync(CoordinadorDgaa coordinadorDgaa)
        {
            _context.CoordinadorDgaa.Update(coordinadorDgaa);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(CoordinadorDgaa coordinadorDgaa)
        {
            _context.CoordinadorDgaa.Remove(coordinadorDgaa);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EsSuperUsuarioAsync(string correo)
        {
            var superUsuario = _context.SuperUsuario.FirstOrDefault(u => u.Correo == correo);


            return superUsuario != null;
        }
    }
}
