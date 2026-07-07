using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public class OfertaRepository : IOfertaRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public OfertaRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<List<Oferta>> ObtenerPorAvisoAsync(int idAviso)
        {
            var idsOferta = await _context.OfertaAviso
                .Where(oa => oa.IdAviso == idAviso)
                .Select(oa => oa.IdOferta)
                .ToListAsync();

            var ofertas = new List<Oferta>();
            foreach (var id in idsOferta)
            {
                if (id > 0)
                {
                    var oferta = await _context.Oferta
                        .Include(o => o.IdExperienciaEducativaNavigation)
                        .Include(o => o.IdExperienciaEducativaNavigation.IdPlanEstudiosNavigation)
                        .FirstOrDefaultAsync(o => o.IdOferta == id);
                    if (oferta is not null)
                        ofertas.Add(oferta);
                }
            }

            return ofertas;
        }
    }
}
