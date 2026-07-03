using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Models.DTOs.Aviso;
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

        public async Task<List<Aviso>> ObtenerTodosAsync(FiltroAvisosDTO filtro)
        {
            var query = _context.Aviso
                .Include(a => a.IdEntidadAcademicaNavigation)
                .Include(a => a.IdArticuloNavigation)
                .AsNoTracking()
                .AsQueryable();

            if (filtro.IdEntidadAcademica > 0)
                query = query.Where(a => a.IdEntidadAcademica == filtro.IdEntidadAcademica);
            if (!string.IsNullOrEmpty(filtro.Busqueda))
                query = query.Where(a => a.Folio.Contains(filtro.Busqueda));
            if (filtro.IdPeriodo > 0)
                query = query.Where(a => a.IdPeriodo == filtro.IdPeriodo);
            //TODO
            /*if (filtro.FechaInicio.)
                todos = todos.Where(a => a.FechaInicio)*/

            var skip = (filtro.Pagina - 1) * filtro.Cantidad;

            return await query.Skip(skip).ToListAsync();
        }

        public async Task<Aviso?> ObtenerPorIDAsync(int idAviso)
        {
            return await _context.Aviso
                .AsNoTracking()
                .Include(a => a.IdEntidadAcademicaNavigation)
                .Include(a => a.IdArticuloNavigation)
                .FirstOrDefaultAsync(aviso => aviso.IdAviso == idAviso);
        }

        public async Task CrearAsync(Aviso aviso)
        {
            if (aviso == null)
                return;
            await _context.Aviso.AddAsync(aviso);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int idAviso)
        {
            var aviso = await _context.Aviso
                .FirstOrDefaultAsync(aviso => aviso.IdAviso == idAviso);
            _context.Aviso.Remove(aviso);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Aviso aviso)
        {
            _context.Aviso.Update(aviso);
            await _context.SaveChangesAsync();
        }

        public async Task<int> ContarAsync(FiltroAvisosDTO filtro)
        {
            var query = _context.Aviso
                .AsNoTracking()
                .AsQueryable();

            if (filtro.IdEntidadAcademica > 0)
                query = query.Where(a => a.IdEntidadAcademica == filtro.IdEntidadAcademica);
            if (!string.IsNullOrEmpty(filtro.Busqueda))
                query = query.Where(a => a.Folio.Contains(filtro.Busqueda));
            if (filtro.IdPeriodo > 0)
                query = query.Where(a => a.IdPeriodo == filtro.IdPeriodo);
            //TODO
            /*if (filtro.FechaInicio.)
                todos = todos.Where(a => a.FechaInicio)*/

            return await query.CountAsync();
        }
    }
}
