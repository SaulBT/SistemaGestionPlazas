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

        public async Task<Articulo?> ExisteAsync(string numero)
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

        public async Task<Articulo?> ActualizarArticuloAsync(Articulo articulo)
        {
            var actualizado = await _context.Articulo.FindAsync(articulo.IdArticulo);

            if (actualizado is null)
                return null;

            actualizado.Numero = articulo.Numero;
            actualizado.Descripcion = articulo.Descripcion;
            await _context.SaveChangesAsync();

            return actualizado;
        }

        public async Task<IEnumerable<Articulo>> BuscarPorTerminoAsync(string busqueda)
        {
            var texto = busqueda.Trim();

            var query = _context.Articulo.AsQueryable();

            if (!string.IsNullOrEmpty(texto))
            {
                query = query.Where(a =>
                    a.Descripcion.Contains(texto) ||
                    a.Numero.Contains(texto)
                );
            }

            var resultados = await query.ToListAsync();

            return resultados;
        }
    }
}
