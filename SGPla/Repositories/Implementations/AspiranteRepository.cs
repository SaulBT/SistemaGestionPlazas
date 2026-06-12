using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public class AspiranteRepository: IAspiranteRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public AspiranteRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarAsync(Docente docente)
        {
            await _context.Docente.AddAsync(docente);
            await _context.SaveChangesAsync();
        }

        public async Task<Docente?> ObtenerPorIdAsync(int idDocente)
        {
            return await _context.Docente
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.IdDocente == idDocente);
        }

        public async Task<List<Docente>> ObtenerTodosAsync()
        {
            return await _context.Docente
                .AsNoTracking()
                .Where(d => string.IsNullOrEmpty(d.NumeroPersonal))
                .ToListAsync();
        }

        public async Task<List<Docente>> ObtenerPorPaginaAsync(string busqueda, int pagina, int cantidad)
        {
            var skip = (pagina - 1) * cantidad;

            var lista = await _context.Docente
                .AsNoTracking()
                .Where(d => string.IsNullOrEmpty(d.NumeroPersonal))
                .ToListAsync();

            if (!string.IsNullOrEmpty(busqueda))
                lista = lista.Where(d =>
                        d.Nombre.Contains(busqueda))
                    .ToList();

            return lista.Skip(skip).ToList();
        }

        public async Task EditarAsync(Docente docenteEditado)
        {
            var docenteOriginal = await _context.Docente.FirstOrDefaultAsync(d => d.IdDocente == docenteEditado.IdDocente);
            docenteOriginal.Nombre = docenteEditado.Nombre;
            docenteOriginal.DescripcionPerfil = docenteEditado.DescripcionPerfil;
            docenteOriginal.IdArchivosGenerales = docenteEditado.IdArchivosGenerales;
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Docente docente)
        {
            _context.Docente.Remove(docente);
            await _context.SaveChangesAsync();
        }

        public async Task<int> ContarAsync(string busqueda)
        {
            var total = 0;
            var lista = await _context.Docente
                .Where(d => string.IsNullOrEmpty(d.NumeroPersonal))
                .ToListAsync();
            if (!string.IsNullOrEmpty(busqueda))
                total = lista
                    .Where(d =>
                        d.Nombre.Contains(busqueda))
                    .Count();
            else
                total = lista.Count();

            return total;
        }

        public async Task<bool> ExistePorIdAsync(int idDocente)
        {
            return await _context.Docente.AnyAsync(d => d.IdDocente == idDocente);
        }
    }
}
