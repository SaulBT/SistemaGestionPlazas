using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public class AvisoRepository : IAvisoRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public AvisoRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<List<Aviso>> ObtenerTodos()
        {
            return await _context.Aviso
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Aviso?> ObtenerPorID(int idAviso)
        {
            return await _context.Aviso
                .AsNoTracking()
                .FirstOrDefaultAsync(aviso => aviso.IdAviso == idAviso);
        }

        public async Task<Aviso> CrearAviso(Aviso aviso)
        {
            if (aviso == null)
                return null;
            await _context.Aviso.AddAsync(aviso);
            await _context.SaveChangesAsync();
            return aviso;
        }

        public async Task AsociarOfertasPorAviso(List<int> idsOfertas, int idAviso)
        {
            foreach (int id in idsOfertas)
            {
                await _context.OfertaAviso.AddAsync(new OfertaAviso
                {
                    IdAviso = idAviso,
                    IdOferta = id
                });

            }
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAvisoPorId(int idAviso)
        {
            var aviso = await _context.Aviso
                .FirstOrDefaultAsync(aviso => aviso.IdAviso == idAviso);
            _context.Aviso.Remove(aviso);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAvisoPorId(Aviso aviso)
        {
            _context.Aviso.Update(aviso);
            await _context.SaveChangesAsync();
        }

    }
}
