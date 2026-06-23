using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public class GradoRepository : IGradoRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public GradoRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task AgregarAsync(Grado grado)
        {
            _context.Grado.Add(grado);
            await _context.SaveChangesAsync();
        }

        public async Task<Grado?> ObtenerAsync(int idGrad)
        {
            return await _context.Grado.FindAsync(idGrad);
        }

        public async Task<List<Grado>> ObtenerTodosAsync(int idDocente)
        {
            return await _context.Grado
                .Where(g => g.IdDocente == idDocente)
                .ToListAsync();
        }

        public async Task EditarAsync(Grado gradoEditado)
        {
            var gradoOriginal = await _context.Grado.FindAsync(gradoEditado.IdGrado);
            gradoOriginal.Grado1 = gradoEditado.Grado1;
            gradoOriginal.Titulo = gradoEditado.Titulo;
            gradoOriginal.Ultimo = gradoEditado.Ultimo;
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Grado grado)
        {
            _context.Grado.Remove(grado);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExsiteAsync(int idGrado)
        {
            return await _context.Grado.AnyAsync(g => g.IdGrado == idGrado);
        }
    }
}
