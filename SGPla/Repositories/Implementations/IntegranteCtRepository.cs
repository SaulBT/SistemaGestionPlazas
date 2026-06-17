using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public class IntegranteCtRepository : IIntegranteCtRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public IntegranteCtRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarAsync(IntegranteCt integrante)
        {
            await _context.IntegranteCt.AddAsync(integrante);
            await _context.SaveChangesAsync();
        }

        public async Task EditarAsync(IntegranteCt integrante)
        {
            var integranteExistente = await _context.IntegranteCt.Where(i => i.IdIntegranteCt == integrante.IdIntegranteCt).FirstOrDefaultAsync();
            integranteExistente.Cargo = integrante.Cargo;
            integranteExistente.Nombre = integrante.Nombre;
            await _context.SaveChangesAsync();
        }

        public async Task<List<IntegranteCt>> ObtenerTodosAsync(int idEntidadAcademica)
        {
            return await _context.IntegranteCt.Where(i => i.IdEntidadAcademica == idEntidadAcademica)
                .AsNoTracking()
                .OrderBy(i => i.Nombre)
                .ToListAsync();
        }

        public async Task<List<IntegranteCt>> ObtenerPorPaginaAsync(int idEntidadAcademica, int pagina, int cantidad)
        {
            int skip = (pagina - 1) * cantidad;

            return await _context.IntegranteCt.Where(i => i.IdEntidadAcademica == idEntidadAcademica)
                .Skip(skip)
                .Take(cantidad)
                .AsNoTracking()
                .OrderBy(i => i.Nombre)
                .ToListAsync();
        }

        public async Task<List<IntegranteCt>> ObtenerPorNombrePaginaAsync(int idEntidadAcademica, string nombre, int pagina, int cantidad)
        {
            int skip = (pagina - 1) * cantidad;

            return await _context.IntegranteCt.Where(i => i.IdEntidadAcademica == idEntidadAcademica && i.Nombre.Contains(nombre))
                .Skip(skip)
                .AsNoTracking()
                .OrderBy(i => i.Nombre)
                .ToListAsync();
        }

        public async Task<IntegranteCt?> ObtenerPorIdAsync(int idIntegranteCt)
        {
            return await _context.IntegranteCt.AsNoTracking().FirstOrDefaultAsync(i => i.IdIntegranteCt == idIntegranteCt);
        }

        public async Task EliminarAsync(IntegranteCt integrante)
        {
            _context.IntegranteCt.Remove(integrante);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistePorIdAsync(int idIntegranteCt)
        {
            if (idIntegranteCt == 0) return false;

            return await _context.IntegranteCt.AnyAsync(i => i.IdIntegranteCt == idIntegranteCt);
        }

        public async Task<bool> ExistePorNombre(string nombre, int idEntidadAcademica, int idIntegrante)
        {
            if (nombre == null) return false;

            int coincidencias = await _context.IntegranteCt.Where(i => i.IdEntidadAcademica == idEntidadAcademica && (i.IdIntegranteCt != idIntegrante) && i.Nombre.EndsWith(". "+nombre)).CountAsync();
            return (coincidencias > 0);
        }

        public async Task<int> ContarAsync(int idEntidadAcademica)
        {
            return await _context.IntegranteCt.Where(i => i.IdEntidadAcademica == idEntidadAcademica).CountAsync();
        }

        public async Task<int> ContarAsync(int idEntidadAcademica, string busqueda)
        {
            return await _context.IntegranteCt.Where(i => i.IdEntidadAcademica == idEntidadAcademica && i.Nombre.Contains(busqueda)).CountAsync();
        }
    }
}
