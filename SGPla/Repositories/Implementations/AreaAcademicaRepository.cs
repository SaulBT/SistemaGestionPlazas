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
        public async Task<List<AreaAcademica>> ObtenerTodosOpcionesAsync()
        {
            return await _context.AreaAcademica
                .AsNoTracking()
                .OrderBy(a => a.Nombre).Select(e => new AreaAcademica
                {
                    IdAreaAcademica = e.IdAreaAcademica,
                    Nombre = e.Nombre
                })
                .ToListAsync();
        }

        public async Task<List<AreaAcademica>> ObtenerPorNombreAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return new List<AreaAcademica>();

            nombre = nombre.Trim();

            return await _context.AreaAcademica
                .AsNoTracking()
                .Where(a => a.Nombre.Contains(nombre))
                .OrderBy(a => a.Nombre)
                .ToListAsync();
        }
        

        public async Task<List<AreaAcademica>> ObtenerPorFiltroAsync(string busqueda, int pagina, int cantidad)
        {
            var query = _context.AreaAcademica.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.Trim();
                query = query.Where(a => a.Nombre.Contains(busqueda));
            }

            query = query.OrderBy(a => a.Nombre);

            int skip = (pagina - 1) * cantidad;

            return await query
                .Skip(skip)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<int> ContarPorFiltroAsync(string busqueda)
        {
            var query = _context.AreaAcademica.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.Trim();
                query = query.Where(a => a.Nombre.Contains(busqueda));
            }
            return await query.CountAsync();
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

        public async Task EliminarAsync(AreaAcademica areaAcademica)
        {
            ArgumentNullException.ThrowIfNull(areaAcademica);

            _context.AreaAcademica.Remove(areaAcademica);
            await _context.SaveChangesAsync();
        }
    }
}
