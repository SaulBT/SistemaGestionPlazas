using SGPla.Data;
using SGPla.Models;
using Microsoft.EntityFrameworkCore;
using SGPla.Repositories.Interfaces;


namespace SGPla.Repositories.Implementations
{
    public class ArticuloRepository : IArticuloRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public ArticuloRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<Articulo> CrearArticuloAsync(Articulo articulo)
        {
            _context.Articulo.Add(articulo);
            await _context.SaveChangesAsync();
            return articulo;
        }

        public async Task<Articulo?> ExisteNumeroAsync(string numero)
        {
            return await _context.Articulo.FirstOrDefaultAsync(a => a.Numero == numero);
        }

        public async Task<bool> EliminarArticuloAsync(int id)
        {
            var articulo = await ObtenerArticuloPorIdAsync(id);

            if (articulo is not null)
            {
                _context.Articulo.Remove(articulo);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<Articulo>> ObtenerTodosAsync()
        {
            return await _context.Articulo.OrderBy(a => a.Numero).ToListAsync();
        }

        public Task<Articulo?> ObtenerArticuloPorIdAsync(int id)
        {
            return _context.Articulo.FindAsync(id).AsTask();
        }

        public async Task<Articulo?> EditarArticuloAsync(Articulo articulo)
        {
            var toUpdate = await _context.Articulo.FindAsync(articulo.IdArticulo);

            if (toUpdate is null)
                return null;

            toUpdate.Numero = articulo.Numero;
            toUpdate.Descripcion = articulo.Descripcion;

            await _context.SaveChangesAsync();

            return toUpdate;
        }
    }
}
