using SGPla.Data;
using SGPla.Models;
using SGPla.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace SGPla.Repositories.Implementations
{
    public class DocenteRepository : IDocenteRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public DocenteRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, int>>
      ObtenerIdsPorNumeroPersonalAsync(List<string> numerosPersonal)
        {
            return await _context.Docente
                .Where(d => numerosPersonal.Contains(d.NumeroPersonal))
                .ToDictionaryAsync(
                    d => d.NumeroPersonal,
                    d => d.IdDocente
                );
        }

        public async Task<List<string>> ObtenerNumerosPersonalRegistradosAsync(
        List<string> numerosPersonal)
        {
            return await _context.Docente
                .Where(d => numerosPersonal.Contains(d.NumeroPersonal))
                .Select(d => d.NumeroPersonal)
                .ToListAsync();
        }
        public async Task<Docente> RegistrarAsync(Docente docente)
        {
            await _context.Docente.AddAsync(docente);
            await _context.SaveChangesAsync();
            return docente;
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
                .Where(d => !string.IsNullOrEmpty(d.NumeroPersonal))
                .ToListAsync();
        }

        public async Task<List<Docente>> ObtenerPorPaginaAsync(string busqueda, int pagina, int cantidad)
        {
            var skip = (pagina - 1) * cantidad;

            var lista = await _context.Docente
                .AsNoTracking()
                .Where(d => !string.IsNullOrEmpty(d.NumeroPersonal))
                .ToListAsync();

            if (!string.IsNullOrEmpty(busqueda))
                lista = lista.Where(d =>
                        d.Nombre.Contains(busqueda) ||
                        d.NumeroPersonal?.Contains(busqueda) == true)
                    .ToList();

            return lista.Skip(skip).ToList();
        }

        public async Task EditarAsync(Docente docenteEditado)
        {
            var docenteOriginal = await _context.Docente.FirstOrDefaultAsync(d => d.IdDocente == docenteEditado.IdDocente);
            docenteOriginal.Nombre = docenteEditado.Nombre;
            docenteOriginal.DescripcionPerfil = docenteEditado.DescripcionPerfil;
            docenteOriginal.IdArchivosGenerales = docenteEditado.IdArchivosGenerales;
            docenteOriginal.NumeroPersonal = docenteEditado.NumeroPersonal;
            docenteOriginal.Puesto = docenteEditado.Puesto;
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
                .Where(d => !string.IsNullOrEmpty(d.NumeroPersonal))
                .ToListAsync();
            if (!string.IsNullOrEmpty(busqueda))
                total = lista
                    .Where(d =>
                        d.Nombre.Contains(busqueda) ||
                        d.NumeroPersonal?.Contains(busqueda) == true)
                    .Count();
            else
                total = lista.Count();

            return total;
        }

        public async Task<bool> ExistePorIdAsync(int idDocente)
        {
            return await _context.Docente.AnyAsync(d => d.IdDocente == idDocente);
        }

        public async Task<bool> ExistePorNumeroAsync(string numeroPersonal)
        {
            return await _context.Docente.AnyAsync(d => (!string.IsNullOrEmpty(d.NumeroPersonal) && d.NumeroPersonal.Contains(numeroPersonal)));
        }
    }
}
