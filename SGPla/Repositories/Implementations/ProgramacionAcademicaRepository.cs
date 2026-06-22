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

        public async Task GuardarOfertas(List<Oferta> ofertas)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                await _context.Oferta.AddRangeAsync(ofertas);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }



        public async Task<List<string>> ObtenerRelacionesValidasAsync(List<OfertaDTO> ofertas)
        {
            var relaciones = ofertas
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Programa) &&
                    !string.IsNullOrWhiteSpace(x.ExperienciaEducativa))
                .Select(x => $"{x.Programa}|{x.ExperienciaEducativa}")
                .Distinct()
                .ToList();

            var relacionesValidas = await _context.ExperienciaEducativa
                .Select(ee =>
                    ee.IdPlanEstudiosNavigation
                        .IdProgramaEducativoNavigation.Nombre
                    + "|" +
                    ee.Nombre)
                .Where(relacion => relaciones.Contains(relacion))
                .Distinct()
                .ToListAsync();

            return relacionesValidas;
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
                .OrderBy(r => r.EntidadAcademica)
                .ThenBy(r => r.ProgramaEducativo)
                .ToListAsync();

            return resumen;
        }


        public async Task<List<OfertaDTO>> ObtenerOfertasGuardadasAsync(
    int idEntidadAcademica, int idProgramaEducativo, int idPeriodo)
        {
            return await _context.Oferta
                .Include(o => o.IdProgramaEducativoNavigation)
                .Where(o =>
                    o.IdProgramaEducativo == idProgramaEducativo &&
                    o.IdPeriodo == idPeriodo &&
                    o.IdProgramaEducativoNavigation.IdEntidadAcademica == idEntidadAcademica)
                .Select(o => new OfertaDTO
                {
                    Programa = o.IdProgramaEducativoNavigation.Nombre,
                    ExperienciaEducativa = o.IdExperienciaEducativaNavigation.Nombre,
                    NRC = o.Nrc,
                    HorasPago = o.Hsm,
                    TC = o.TipoContratacion,
                    NombreDocente = o.IdDocenteNavigation.Nombre,
                    NP = o.IdDocenteNavigation.NumeroPersonal,
                    Articulo = o.IdArticulo,
                    IdPeriodo = o.IdPeriodo,
                    Region = o.IdProgramaEducativoNavigation.IdEntidadAcademicaNavigation.Region,
                })
                .ToListAsync();
        }
    }
}