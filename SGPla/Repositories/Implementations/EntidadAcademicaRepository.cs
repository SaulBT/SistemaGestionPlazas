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

        public async Task<List<EntidadAcademica>> ObtenerOpcionesAsync(string region, int idAreaAcademica)
        {
            return await _context.EntidadAcademica
            .AsNoTracking()
            .OrderBy(e => e.Nombre).Where(e => e.Region == region && e.IdAreaAcademica == idAreaAcademica)
            .Select(e => new EntidadAcademica
            {
                IdEntidadAcademica = e.IdEntidadAcademica,
                Nombre = e.Nombre
            })
            .ToListAsync();
        }

        public async Task<List<EntidadAcademica>> ObtenerDiezAsync(int indiceInicial)
        {
            return await _context.EntidadAcademica
                .AsNoTracking()
                .OrderBy(e => e.Nombre)
                .Include(e => e.IdAreaAcademicaNavigation)
                .Skip(indiceInicial - 1)
                .Take(10)
                .ToListAsync();
        }

        public async Task<List<EntidadAcademica>> ObtenerPorFiltroAsync(string? region, int? idAreaAcademica, string? nombre, int pagina, int cantidad)
        {
            var lista = _context.EntidadAcademica
                .Include(e => e.IdAreaAcademicaNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(region))
                lista = lista.Where(e => e.Region == region);
            if (idAreaAcademica != null && idAreaAcademica >= 0)
                lista = lista.Where(e=> e.IdAreaAcademica == idAreaAcademica);
            if (!string.IsNullOrEmpty(nombre))
                lista = lista.Where(e => e.Nombre.Contains(nombre));

            return await lista.Skip((pagina - 1) * cantidad)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<EntidadAcademica?> ObtenerPorIdAsync(int idEntidadAcademica)
        {
            return await _context.EntidadAcademica
                .Include(e => e.IdAreaAcademicaNavigation)
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

        public async Task<List<EntidadAcademica>> ObtenerPorIdAreaAcademicaYRegionAsync(int idAreaAcademica, string region)
        {
            if (idAreaAcademica <= 0)
                return new List<EntidadAcademica>();

            return await _context.EntidadAcademica
                .AsNoTracking()
                .Where(e => e.IdAreaAcademica == idAreaAcademica && e.Region == region)
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

        public async Task<bool> ExistePorClaveAsync(string clave)
        {
            string prefijoClave = $"{clave.Trim()}-";

            return await _context.EntidadAcademica
                .AnyAsync(e => e.Nombre != null && e.Nombre.StartsWith(prefijoClave));
        }

        public async Task<bool> ExistePorClaveAsync(string clave, int idEntidadAcademica)
        {
            string prefijoClave = $"{clave.Trim()}-";

            return await _context.EntidadAcademica
                .AnyAsync(e => e.IdEntidadAcademica != idEntidadAcademica
                    && e.Nombre != null
                    && e.Nombre.StartsWith(prefijoClave));
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

        public async Task EliminarAsync(EntidadAcademica entidadAcademica)
        {
            ArgumentNullException.ThrowIfNull(entidadAcademica);

            _context.EntidadAcademica.Remove(entidadAcademica);
            await _context.SaveChangesAsync();
        }

        public async Task<int> ContarPorFiltroAsync(string? region, int? idAreaAcademica, string? nombre)
        {
            var query = _context.EntidadAcademica
                .Include(e => e.IdAreaAcademicaNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(region))
                query = query.Where(e => e.Region == region);
            if (idAreaAcademica != null && idAreaAcademica >= 0)
                query = query.Where(e => e.IdAreaAcademica == idAreaAcademica);
            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(e => e.Nombre.Contains(nombre));
            return await query.CountAsync();
        }
    }
}
