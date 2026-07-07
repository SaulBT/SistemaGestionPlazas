using Microsoft.EntityFrameworkCore;
using SGPla.Commons;
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
            if (filtro.IdAreaAcademica > 0)
                query = query.Where(a => a.IdEntidadAcademicaNavigation.IdAreaAcademica == filtro.IdAreaAcademica);
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

        public async Task CambiarStatusArchivadoAsync(int idAviso, bool archivado)
        {
            var aviso = _context.Aviso.FirstOrDefault(a => a.IdAviso == idAviso);
            if (aviso is not null)
            {
                aviso.Archivado = archivado;
                await _context.SaveChangesAsync();
            }

        }

        public async Task EnviarARevisionAsync(int idAviso, string comentarios)
        {
            var aviso = _context.Aviso.FirstOrDefault(a => a.IdAviso == idAviso);
            if (aviso is not null)
            {
                aviso.Estado = Constantes.EN_REVISION_POR_DGAA;
                aviso.Comentarios = comentarios;
                await _context.SaveChangesAsync();
            }
        }

        public async Task FirmarAsync(int idAviso, int idArchivoFirmado)
        {
            var aviso = _context.Aviso.FirstOrDefault(a => a.IdAviso == idAviso);
            if (aviso is not null)
            {
                aviso.Estado = Constantes.FIRMADO;
                aviso.IdArchivoFirmado = idArchivoFirmado;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string> VerComentariosAsync(int idAviso)
        {
            var comentarios = "";
            var aviso = _context.Aviso.FirstOrDefault(a => a.IdAviso == idAviso);
            if (aviso is not null)
                comentarios = aviso.Comentarios ?? "";

            return comentarios;
        }

        public async Task PublicarAsync(int idAviso, string url)
        {
            var aviso = _context.Aviso.FirstOrDefault(a => a.IdAviso == idAviso);
            if (aviso is not null)
            {
                aviso.Estado = Constantes.PUBLICADO;
                aviso.UrlPublicacion = url;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> VerificarEstadoAsync(int idAviso, string estado)
        {
            var aviso = _context.Aviso.FirstOrDefault(a => a.IdAviso == idAviso);
            if (aviso is not null)
                return aviso.Estado == estado;

            return false;
        }

        public async Task<bool> ExistePorId(int idAviso)
        {
            var aviso = _context.Aviso.FirstOrDefault(a => a.IdAviso == idAviso);
            return aviso is not null;
        }
    }
}
