using Microsoft.EntityFrameworkCore;
using SGPla.Commons;
using SGPla.Data;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Planea;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public sealed class ConsultaPlaneaRepository(GestionDePlazasDbContext context) : IConsultaPlaneaRepository
    {
        private readonly GestionDePlazasDbContext _context = context;
        public async Task<(List<BitacoraPlaneaDTO> Items, int Total)> ObtenerBitacoraAsync(FiltroBitacoraPlaneaDTO f, CancellationToken cancellationToken = default)
        {
            var q = _context.SincronizacionPlanea.AsNoTracking().AsQueryable();
            if (f.IdPeriodo.HasValue) q = q.Where(x => x.IdPeriodo == f.IdPeriodo);
            if (!string.IsNullOrEmpty(f.Estado)) q = q.Where(x => x.Estado == f.Estado);
            var total = await q.CountAsync(cancellationToken);
            var items = await q.OrderByDescending(x => x.FechaInicio).Skip((f.Pagina - 1) * f.Cantidad).Take(f.Cantidad)
                .Select(x => new BitacoraPlaneaDTO(x.IdSincronizacionPlanea, x.IdPeriodoNavigation.Codigo.Trim(), x.FechaInicio, x.FechaFin, x.Estado,
                    x.RegistrosRecibidos, x.NrcRecibidos, x.NrcNuevos, x.NrcExistentes, x.NrcSinPlan, x.NrcSinExperiencia, x.HorariosInsertados, x.Advertencias, x.MensajeError)).ToListAsync(cancellationToken);
            return (items,total);
        }
        public async Task<(List<CopiaPlaneaFilaDTO> Items, int Total)> ObtenerCopiasAsync(FiltroCopiaPlaneaDTO f, CancellationToken cancellationToken = default)
        {
            var q = _context.ExperienciaEducativaPeriodo.AsNoTracking().AsQueryable();
            if (f.IdPeriodo.HasValue) q = q.Where(x => x.IdPeriodo == f.IdPeriodo);
            if (f.IdPlanEstudios.HasValue) q = q.Where(x => x.IdPlanEstudios == f.IdPlanEstudios);
            if (f.IdRegion.HasValue) q = q.Where(x => x.IdRegion == f.IdRegion);
            if (!string.IsNullOrEmpty(f.Busqueda)) q = q.Where(x => x.Nrc.Contains(f.Busqueda) || x.IdExperienciaEducativaNavigation.Codigo.Contains(f.Busqueda) || x.IdExperienciaEducativaNavigation.Nombre.Contains(f.Busqueda));
            var total = await q.CountAsync(cancellationToken);
            var items = await q.OrderBy(x => x.Nrc).Skip((f.Pagina - 1) * f.Cantidad).Take(f.Cantidad)
                .Select(x => new CopiaPlaneaFilaDTO(x.IdExperienciaEducativaPeriodo, x.IdPeriodoNavigation.Codigo.Trim(), x.Nrc,
                    x.IdExperienciaEducativaNavigation.Codigo, x.IdExperienciaEducativaNavigation.Nombre, x.IdPlanEstudiosNavigation.CodigoPlan,
                    x.IdRegionNavigation == null ? null : x.IdRegionNavigation.Nombre, x.Campus,
                    x.Horarios.Count, x.FechaAlta)).ToListAsync(cancellationToken);
            return (items,total);
        }
        public async Task<DetalleCopiaPlaneaDTO?> ObtenerDetalleCopiaAsync(int idExperienciaEducativaPeriodo, CancellationToken cancellationToken = default)
        {
            var copia = await _context.ExperienciaEducativaPeriodo.AsNoTracking().Where(x => x.IdExperienciaEducativaPeriodo == idExperienciaEducativaPeriodo)
                .Select(x => new { Fila = new CopiaPlaneaFilaDTO(x.IdExperienciaEducativaPeriodo, x.IdPeriodoNavigation.Codigo.Trim(), x.Nrc,
                    x.IdExperienciaEducativaNavigation.Codigo, x.IdExperienciaEducativaNavigation.Nombre, x.IdPlanEstudiosNavigation.CodigoPlan,
                    x.IdRegionNavigation == null ? null : x.IdRegionNavigation.Nombre, x.Campus, x.Horarios.Count, x.FechaAlta),
                    x.Titulo, NombrePlan = x.IdPlanEstudiosNavigation.Nombre, x.Nivel, x.Area, x.IdSincronizacionPlanea,
                    Horarios = x.Horarios.Select(h => new HorarioCopiaDTO(h.Dia, h.HoraInicio, h.HoraFin, h.Edificio, h.Aula, h.FechaInicio, h.FechaFin)).ToList() })
                .FirstOrDefaultAsync(cancellationToken);
            if (copia is null) return null;
            var orden = copia.Horarios.OrderBy(h => PlaneaConstantes.DIAS.ToList().IndexOf(h.Dia)).ThenBy(h => h.HoraInicio).ToList();
            return new(copia.Fila, copia.Titulo, copia.NombrePlan, copia.Nivel, copia.Area, copia.IdSincronizacionPlanea, orden);
        }
        public Task<List<OptionModel>> ObtenerPeriodosConSincronizacionAsync(CancellationToken cancellationToken = default) => _context.Periodo.AsNoTracking()
            .Where(p => p.SincronizacionPlanea.Any() || p.ExperienciaEducativaPeriodo.Any()).OrderByDescending(p => p.Codigo)
            .Select(p => new OptionModel { Value = p.IdPeriodo.ToString(), Text = p.Codigo.Trim() }).ToListAsync(cancellationToken);
        public Task<List<OptionModel>> ObtenerPlanesConCopiasAsync(int? idPeriodo, CancellationToken cancellationToken = default) => _context.PlanEstudios.AsNoTracking()
            .Where(p => p.ExperienciaEducativaPeriodo.Any(c => !idPeriodo.HasValue || c.IdPeriodo == idPeriodo))
            .OrderBy(p => p.CodigoPlan).Select(p => new OptionModel { Value = p.IdPlanEstudios.ToString(), Text = (p.CodigoPlan ?? "") + " - " + p.Nombre }).ToListAsync(cancellationToken);
        public Task<List<OptionModel>> ObtenerRegionesAsync(CancellationToken cancellationToken = default) => _context.Region.AsNoTracking().OrderBy(r => r.Nombre)
            .Select(r => new OptionModel { Value = r.IdRegion.ToString(), Text = r.Nombre }).ToListAsync(cancellationToken);
    }
}
