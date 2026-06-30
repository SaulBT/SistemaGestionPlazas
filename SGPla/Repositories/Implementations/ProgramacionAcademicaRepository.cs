using SGPla.Data;
using SGPla.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models;
using SGPla.Models.DTOs.ProgramacionAcademica;


namespace SGPla.Repositories.Implementations
{
    public class ProgramacionAcademicaRepository : IProgramacionAcademicaRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public ProgramacionAcademicaRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task GuardarOfertasYCargas(List<Oferta> ofertas, List<CargaAcademica> cargas)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Oferta.AddRangeAsync(ofertas);
                await _context.CargaAcademica.AddRangeAsync(cargas);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static string ObtenerClavePrograma(string nombre)
        {
            return nombre.Split('-')[0].Trim();
        }

        public async Task<List<string>> ObtenerRelacionesValidasAsync(List<OfertaDTO> ofertas)
        {
            var relacionesArchivo = ofertas
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Programa) &&
                    !string.IsNullOrWhiteSpace(x.ExperienciaEducativa))
                .Select(x =>
                    $"{ObtenerClavePrograma(x.Programa)}|{x.ExperienciaEducativa}")
                .Distinct()
                .ToHashSet();

            var relacionesBd = await _context.ExperienciaEducativa
                .Select(ee => new
                {
                    Programa =
                        ee.IdPlanEstudiosNavigation
                          .IdProgramaEducativoNavigation.Nombre,
                    Experiencia = ee.Nombre
                })
                .ToListAsync();

            return relacionesBd
                .Select(x =>
                    $"{ObtenerClavePrograma(x.Programa)}|{x.Experiencia}")
                .Where(relacionesArchivo.Contains)
                .Distinct()
                .ToList();
        }

        public async Task<List<ResumenOfertaProgramacionAcademicaDTO>> ObtenerResumenPorProgramaPeriodoAsync(BuscarProgramacionAcademicaDTO? filtro)
        {
            IQueryable<Oferta> query = _context.Oferta
    .Include(o => o.IdProgramaEducativoNavigation)
        .ThenInclude(pe => pe.IdEntidadAcademicaNavigation)
    .Include(o => o.IdPeriodoNavigation);

            if (filtro != null)
            {
                if (filtro.IdPeriodo.HasValue)
                    query = query.Where(o => o.IdPeriodo == filtro.IdPeriodo.Value);

                if (filtro.IdEntidadAcademica.HasValue)
                    query = query.Where(o => o.IdProgramaEducativoNavigation.IdEntidadAcademica == filtro.IdEntidadAcademica.Value);

                if (filtro.IdProgramaEducativo.HasValue)
                    query = query.Where(o => o.IdProgramaEducativo == filtro.IdProgramaEducativo.Value);

                if (!string.IsNullOrEmpty(filtro.Region))
                    query = query.Where(o => o.IdProgramaEducativoNavigation.IdEntidadAcademicaNavigation.Region == filtro.Region);

                if (!string.IsNullOrEmpty(filtro.Busqueda))
                    query = query.Where(o => o.IdProgramaEducativoNavigation.Nombre.Contains(filtro.Busqueda) || o.IdProgramaEducativoNavigation.IdEntidadAcademicaNavigation.Nombre.Contains(filtro.Busqueda));
            }

            var resumen = await query
                .GroupBy(o => new
                {
                    o.IdProgramaEducativo,
                    Programa = o.IdProgramaEducativoNavigation.Nombre,
                    o.IdProgramaEducativoNavigation.IdEntidadAcademica,
                    Entidad = o.IdProgramaEducativoNavigation.IdEntidadAcademicaNavigation.Nombre,
                    o.IdPeriodo,
                    Periodo = o.IdPeriodoNavigation.Codigo
                })
                .Select(g => new ResumenOfertaProgramacionAcademicaDTO
                {
                    IdProgramaEducativo = g.Key.IdProgramaEducativo,
                    ProgramaEducativo = g.Key.Programa,
                    IdEntidadAcademica = g.Key.IdEntidadAcademica,
                    EntidadAcademica = g.Key.Entidad,
                    IdPeriodo = g.Key.IdPeriodo,
                    CodigoPeriodo = g.Key.Periodo,
                    EEAsignadas = g.Count(x => x.IdDocente != null),
                    EEVacantes = g.Count(x => x.IdDocente == null),
                    TotalEE = g.Count()
                })
                .OrderByDescending(g => g.CodigoPeriodo)
                .ThenBy(r => r.ProgramaEducativo)
                .ToListAsync();

            return resumen;
        }


        public async Task<List<OfertaDTO>> ObtenerOfertasGuardadasAsync(
     int idEntidadAcademica, int idProgramaEducativo, int idPeriodo)
        {
            var ofertas = await _context.Oferta
                .Include(o => o.IdProgramaEducativoNavigation)
                    .ThenInclude(p => p.IdEntidadAcademicaNavigation)
                .Include(o => o.IdExperienciaEducativaNavigation)
                .Include(o => o.IdDocenteNavigation)
                .Include(o => o.Horario) 
                .Where(o =>
                    o.IdProgramaEducativo == idProgramaEducativo &&
                    o.IdPeriodo == idPeriodo &&
                    o.IdProgramaEducativoNavigation.IdEntidadAcademica == idEntidadAcademica)
                .ToListAsync();

            return ofertas.Select(o => new OfertaDTO
            {
                Programa = o.IdProgramaEducativoNavigation.Nombre,
                ExperienciaEducativa = o.IdExperienciaEducativaNavigation.Nombre,
                NRC = o.Nrc,
                HorasPago = o.Hsm,
                TC = o.TipoContratacion,
                NombreDocente = o.IdDocenteNavigation?.Nombre,
                NP = o.IdDocenteNavigation?.NumeroPersonal,
                Articulo = o.IdArticulo,
                IdPeriodo = o.IdPeriodo,
                Region = o.IdProgramaEducativoNavigation.IdEntidadAcademicaNavigation.Region,

                Lunes = MapHorario(o.Horario, "Lunes"),
                Martes = MapHorario(o.Horario, "Martes"),
                Miercoles = MapHorario(o.Horario, "Miercoles"),
                Jueves = MapHorario(o.Horario, "Jueves"),
                Viernes = MapHorario(o.Horario, "Viernes"),
                Sabado = MapHorario(o.Horario, "Sabado"),
            }).ToList();
        }

        private static HorarioDia? MapHorario(IEnumerable<Horario> horarios, string dia)
        {
            var h = horarios.FirstOrDefault(x =>
                x.Dia.Equals(dia, StringComparison.OrdinalIgnoreCase));

            if (h is null) return null;

            return new HorarioDia
            {
                Inicio = h.HoraInicio.ToTimeSpan(),
                Fin = h.HoraFin.ToTimeSpan()
            };
        }
    }
}