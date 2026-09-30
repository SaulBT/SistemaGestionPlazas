using Microsoft.EntityFrameworkCore;
using SGPla.Commons;
using SGPla.Data;
using SGPla.Models.DTOs.Planea;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public class ProgramacionPlaneaRepository : IProgramacionPlaneaRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public ProgramacionPlaneaRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<UltimaSincronizacionPlaneaDTO?> ObtenerUltimaSincronizacionAsync(
            int? idPeriodo, CancellationToken cancellationToken = default)
        {
            var consulta = _context.SincronizacionPlanea.AsNoTracking();
            if (idPeriodo.HasValue)
                consulta = consulta.Where(s => s.IdPeriodo == idPeriodo.Value);

            return await consulta
                .OrderByDescending(s => s.FechaInicio)
                .ThenByDescending(s => s.IdSincronizacionPlanea)
                .Select(s => new UltimaSincronizacionPlaneaDTO(
                    s.IdPeriodoNavigation.Codigo.Trim(), s.Estado, s.FechaInicio, s.FechaFin,
                    s.NrcRecibidos, s.NrcNuevos, s.NrcExistentes, s.NrcSinPlan, s.NrcSinExperiencia,
                    s.HorariosInsertados, s.MensajeError))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<(List<CopiaProgramacionPlaneaDTO> Copias, int Total)> ObtenerCopiasAsync(
            FiltroProgramacionPlaneaDTO filtro, CancellationToken cancellationToken = default)
        {
            var consulta = _context.ExperienciaEducativaPeriodo.AsNoTracking();

            if (filtro.IdPeriodo.HasValue)
                consulta = consulta.Where(c => c.IdPeriodo == filtro.IdPeriodo.Value);

            if (filtro.IdProgramaEducativo.HasValue)
                consulta = consulta.Where(c => c.IdPlanEstudiosNavigation.IdProgramaEducativo == filtro.IdProgramaEducativo.Value);

            if (filtro.IdEntidadAcademica.HasValue)
                consulta = consulta.Where(c =>
                    c.IdPlanEstudiosNavigation.IdProgramaEducativoNavigation.IdEntidadAcademica == filtro.IdEntidadAcademica.Value);

            if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
                consulta = consulta.Where(c =>
                    c.Nrc.Contains(filtro.Busqueda)
                    || c.IdExperienciaEducativaNavigation.Codigo.Contains(filtro.Busqueda)
                    || c.IdExperienciaEducativaNavigation.Nombre.Contains(filtro.Busqueda));

            var total = await consulta.CountAsync(cancellationToken);

            var copias = await consulta
                .OrderByDescending(c => c.IdPeriodoNavigation.Codigo)
                .ThenBy(c => c.Nrc)
                .Take(filtro.Limite)
                .Select(c => new
                {
                    CodigoPeriodo = c.IdPeriodoNavigation.Codigo.Trim(),
                    c.Nrc,
                    CodigoExperiencia = c.IdExperienciaEducativaNavigation.Codigo,
                    NombreExperiencia = c.IdExperienciaEducativaNavigation.Nombre,
                    c.IdPlanEstudiosNavigation.CodigoPlan,
                    ProgramaEducativo = c.IdPlanEstudiosNavigation.IdProgramaEducativoNavigation.Nombre,
                    Region = c.IdRegionNavigation == null ? null : c.IdRegionNavigation.Nombre,
                    Horarios = c.Horarios
                        .Select(h => new HorarioPlaneaDTO(h.Dia, h.HoraInicio, h.HoraFin, h.Edificio, h.Aula))
                        .ToList()
                })
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

            var resultado = copias
                .Select(c => new CopiaProgramacionPlaneaDTO(
                    c.CodigoPeriodo, c.Nrc, c.CodigoExperiencia, c.NombreExperiencia, c.CodigoPlan,
                    c.ProgramaEducativo, c.Region,
                    c.Horarios
                        .OrderBy(h => IndiceDia(h.Dia))
                        .ThenBy(h => h.HoraInicio)
                        .ToList()))
                .ToList();

            return (resultado, total);
        }

        public async Task<PeriodoPorSincronizar?> ObtenerPeriodoAsync(int idPeriodo, CancellationToken cancellationToken = default)
        {
            return await _context.Periodo.AsNoTracking()
                .Where(p => p.IdPeriodo == idPeriodo)
                .Select(p => new PeriodoPorSincronizar(p.IdPeriodo, p.Codigo.Trim()))
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static int IndiceDia(string dia)
        {
            for (var i = 0; i < PlaneaConstantes.DIAS.Count; i++)
                if (PlaneaConstantes.DIAS[i] == dia) return i;
            return int.MaxValue;
        }
    }
}
