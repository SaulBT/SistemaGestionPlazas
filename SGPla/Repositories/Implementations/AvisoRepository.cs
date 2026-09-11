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
            return await Filtrar(filtro)
                .Include(a => a.IdEntidadAcademicaNavigation)
                .Include(a => a.IdArticuloNavigation)
                .OrderByDescending(a => a.FechaCreacion).ThenByDescending(a => a.IdAviso)
                .ToListAsync();
        }

        private IQueryable<Aviso> Filtrar(FiltroAvisosDTO filtro)
        {
            var query = _context.Aviso.AsNoTracking();
            if (filtro.IdEntidadAcademica > 0)
                query = query.Where(a => a.IdEntidadAcademica == filtro.IdEntidadAcademica);
            if (filtro.IdAreaAcademica > 0)
                query = query.Where(a => a.IdEntidadAcademicaNavigation.IdAreaAcademica == filtro.IdAreaAcademica);
            if (filtro.SoloEnviadosDgaa)
                query = query.Where(a => EstadosAviso.RecibidosDgaa.Contains(a.Estado));
            if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
                query = query.Where(a => a.Folio.Contains(filtro.Busqueda));
            if (filtro.IdPeriodo > 0)
                query = query.Where(a => a.IdPeriodo == filtro.IdPeriodo);
            if (filtro.FechaInicio.HasValue)
                query = query.Where(a => a.FechaCreacion >= filtro.FechaInicio.Value);
            if (filtro.FechaFin.HasValue)
                query = query.Where(a => a.FechaCreacion <= filtro.FechaFin.Value);
            return query;
        }

        public async Task<Aviso?> ObtenerPorIDAsync(int idAviso)
        {
            return await _context.Aviso
                .AsNoTracking()
                .Include(a => a.IdEntidadAcademicaNavigation)
                .Include(a => a.IdArticuloNavigation)
                .FirstOrDefaultAsync(aviso => aviso.IdAviso == idAviso);
        }

        public async Task<Aviso> CrearAsync(Aviso aviso)
        {
            if (aviso == null)
                return null;
            await _context.Aviso.AddAsync(aviso);
            await _context.SaveChangesAsync();
            return aviso;
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
            return await Filtrar(filtro).CountAsync();
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
            await CambiarEstadoRevisionAsync(idAviso, Constantes.EN_REVISION_POR_DGAA, comentarios);
        }

        public async Task CambiarEstadoRevisionAsync(int idAviso, string estado, string comentarios)
        {
            var query = _context.Aviso.Where(a => a.IdAviso == idAviso && a.Archivado != true);
            if (estado == Constantes.EN_REVISION_POR_DGAA)
                query = query.Where(a => a.Estado == Constantes.CREADO || a.Estado == Constantes.DEVUELTO_POR_DGAA);
            else if (estado == Constantes.AVALADO_POR_DGAA || estado == Constantes.DEVUELTO_POR_DGAA)
                query = query.Where(a => a.Estado == Constantes.EN_REVISION_POR_DGAA);
            else throw new ValidacionExcepction("Transición de estado inválida.", "400");

            var cambios = await query.ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.Estado, estado).SetProperty(a => a.Comentarios, comentarios));
            if (cambios == 0)
                throw new ValidacionExcepction("El aviso cambió de estado o está archivado. Actualice el listado.", "409");
        }

        public async Task EditarComentariosRevisionAsync(int idAviso, string comentarios)
        {
            // Actualizar únicamente los comentarios, sin reabrir ni cambiar la decisión.
            // Si la entidad reenvió el aviso, no sobrescribir sus comentarios de envío.
            var cambios = await _context.Aviso
                .Where(a => a.IdAviso == idAviso && EstadosAviso.RevisadosDgaa.Contains(a.Estado))
                .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.Comentarios, comentarios));
            if (cambios == 0)
                throw new ValidacionExcepction("El aviso ya no tiene una revisión editable. Actualice el listado.", "409");
        }

        public async Task FirmarAsync(int idAviso, int idArchivoFirmado)
        {
            var cambios = await _context.Aviso.Where(a => a.IdAviso == idAviso && a.Archivado != true
                && a.Estado == Constantes.AVALADO_POR_DGAA)
                .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.Estado, Constantes.FIRMADO)
                    .SetProperty(a => a.IdArchivoFirmado, idArchivoFirmado));
            if (cambios == 0) throw new ValidacionExcepction("El aviso ya no está disponible para firma. Actualice el listado.", "409");
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
            var cambios = await _context.Aviso.Where(a => a.IdAviso == idAviso && a.Archivado != true
                && a.Estado == Constantes.FIRMADO && a.IdArchivoFirmado != null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.Estado, Constantes.PUBLICADO)
                    .SetProperty(a => a.UrlPublicacion, url));
            if (cambios == 0) throw new ValidacionExcepction("El aviso ya no está disponible para publicación. Actualice el listado.", "409");
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

        public async Task ActualizarCompletoAsync(EditarAvisoDTO avisoDTO, int idArchivoOriginal, List<Horario> horarios)
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync();

            try
            {
                var aviso = await _context.Aviso
                    .FirstOrDefaultAsync(a => a.IdAviso == avisoDTO.IdAviso);

                if (aviso is null)
                    throw new InvalidOperationException("No existe el aviso que se desea actualizar.");

                aviso.IdPeriodo = avisoDTO.IdPeriodo;
                aviso.IdArticulo = avisoDTO.IdArticulo;
                aviso.Folio = avisoDTO.Folio;
                aviso.FechaCt = avisoDTO.FechaCT;
                aviso.FechaVacantes = avisoDTO.FechaVacantes;
                aviso.Requisitos = avisoDTO.Requisitos;
                aviso.Lugar = avisoDTO.Lugar;
                aviso.Correo = avisoDTO.Correo;
                aviso.Modalidad = avisoDTO.Modalidad;
                aviso.Sistema = avisoDTO.Sistema;
                aviso.IdArchivoOriginal = idArchivoOriginal;

                var relacionesAnteriores = await _context.OfertaAviso
                    .Where(oa => oa.IdAviso == avisoDTO.IdAviso)
                    .ToListAsync();
                _context.OfertaAviso.RemoveRange(relacionesAnteriores);

                var horariosAnteriores = await _context.Horario
                    .Where(h => h.IdAviso == avisoDTO.IdAviso)
                    .ToListAsync();
                _context.Horario.RemoveRange(horariosAnteriores);

                await _context.OfertaAviso.AddRangeAsync(avisoDTO.OfertasId.Distinct().Select(idOferta => new OfertaAviso
                {
                    IdAviso = avisoDTO.IdAviso,
                    IdOferta = idOferta
                }));
                await _context.Horario.AddRangeAsync(horarios);

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }
    }
}
