using SGPla.Data;
using SGPla.Models;
using Microsoft.EntityFrameworkCore;

namespace SGPla.Repositories
{
    public class ArticuloRepository : IArticuloRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public ArticuloRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task CrearArticuloAsync(Articulo articulo)
        {
            _context.Articulo.Add(articulo);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarArticuloAsync(int id)
        {
            var articulo = await ObtenerArticuloPorIdAsync(id);
            if (articulo is not null)
            {
                _context.Articulo.Remove(articulo);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Articulo>> ObtenerTodosAsync()
        {
            return await _context.Articulo.OrderBy(a => a.Numero).ToListAsync();
        }

        public Task<Articulo?> ObtenerArticuloPorIdAsync(int id)
        {
            return _context.Articulo.FindAsync(id).AsTask();
        }

        public async Task ActualizarArticuloAsync(Articulo articulo)
        {
            _context.Articulo.Update(articulo);
            await _context.SaveChangesAsync();
        }
    }
}
