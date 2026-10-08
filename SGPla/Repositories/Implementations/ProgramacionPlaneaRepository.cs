using Microsoft.EntityFrameworkCore;
using SGPla.Commons;
using SGPla.Data;
using SGPla.Models;
using SGPla.Models.DTOs.Planea;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;

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

        public async Task<(List<CopiaProgramacionPlaneaDTO> Copias, int Total, int Pagina)> ObtenerCopiasAsync(
            FiltroProgramacionPlaneaDTO filtro, CancellationToken cancellationToken = default)
        {
            // Solo se muestran las copias ya enlazadas con el catálogo; las pendientes esperan su plan de estudios.
            var consulta = _context.ExperienciaEducativaPeriodo.AsNoTracking()
                .Where(c => c.IdExperienciaEducativa != null && c.IdPlanEstudios != null);

            if (filtro.IdPeriodo.HasValue)
                consulta = consulta.Where(c => c.IdPeriodo == filtro.IdPeriodo.Value);

            if (filtro.IdProgramaEducativo.HasValue)
                consulta = consulta.Where(c => c.IdPlanEstudiosNavigation.IdProgramaEducativo == filtro.IdProgramaEducativo.Value);

            if (filtro.IdPlanEstudios.HasValue)
                consulta = consulta.Where(c => c.IdPlanEstudios == filtro.IdPlanEstudios.Value);

            if (filtro.IdEntidadAcademica.HasValue)
                consulta = consulta.Where(c =>
                    c.IdPlanEstudiosNavigation.IdProgramaEducativoNavigation.IdEntidadAcademica == filtro.IdEntidadAcademica.Value);

            if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
                consulta = consulta.Where(c =>
                    c.Nrc.Contains(filtro.Busqueda)
                    || c.IdExperienciaEducativaNavigation.Codigo.Contains(filtro.Busqueda)
                    || c.IdExperienciaEducativaNavigation.Nombre.Contains(filtro.Busqueda));

            var total = await consulta.CountAsync(cancellationToken);

            // Una página fuera de rango (p. ej. tras acotar filtros) se ajusta a la última disponible.
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)filtro.Limite));
            var pagina = Math.Min(filtro.Pagina, totalPaginas);

            var resultado = await ProyectarAsync(
                consulta
                    .OrderByDescending(c => c.IdPeriodoNavigation.Codigo)
                    .ThenBy(c => c.Nrc)
                    .ThenBy(c => c.IdExperienciaEducativaPeriodo)
                    .Skip((pagina - 1) * filtro.Limite)
                    .Take(filtro.Limite),
                cancellationToken);

            return (resultado, total, pagina);
        }

        public async Task<List<CopiaProgramacionPlaneaDTO>> ObtenerCopiasParaAprobarAsync(
            int idPlanEstudios, int idPeriodo, string? busqueda, CancellationToken cancellationToken = default)
        {
            var consulta = _context.ExperienciaEducativaPeriodo.AsNoTracking()
                .Where(c => c.IdExperienciaEducativa != null && c.IdPlanEstudios == idPlanEstudios && c.IdPeriodo == idPeriodo);

            if (!string.IsNullOrWhiteSpace(busqueda))
                consulta = consulta.Where(c =>
                    c.Nrc.Contains(busqueda)
                    || c.IdExperienciaEducativaNavigation!.Codigo.Contains(busqueda)
                    || c.IdExperienciaEducativaNavigation.Nombre.Contains(busqueda));

            return await ProyectarAsync(
                consulta
                    .OrderBy(c => c.EstadoAprobacion == PlaneaConstantes.APROBACION_PENDIENTE ? 0 : 1)
                    .ThenBy(c => c.Nrc)
                    .ThenBy(c => c.IdExperienciaEducativaPeriodo),
                cancellationToken);
        }

        public async Task<ResumenAprobacionPlaneaDTO> ObtenerResumenAprobacionAsync(
            int idPlanEstudios, int idPeriodo, CancellationToken cancellationToken = default)
        {
            var filas = await _context.ExperienciaEducativaPeriodo.AsNoTracking()
                .Where(c => c.IdExperienciaEducativa != null && c.IdPlanEstudios == idPlanEstudios && c.IdPeriodo == idPeriodo)
                .GroupBy(c => c.EstadoAprobacion)
                .Select(g => new { Estado = g.Key, Total = g.Count() })
                .ToListAsync(cancellationToken);

            int Contar(string estado) => filas.Where(f => f.Estado == estado).Sum(f => f.Total);

            var ultima = await _context.ExperienciaEducativaPeriodo.AsNoTracking()
                .Where(c => c.IdPlanEstudios == idPlanEstudios && c.IdPeriodo == idPeriodo && c.FechaRevision != null)
                .OrderByDescending(c => c.FechaRevision)
                .Select(c => new { c.FechaRevision, c.RevisadoPor })
                .FirstOrDefaultAsync(cancellationToken);

            return new ResumenAprobacionPlaneaDTO(
                Contar(PlaneaConstantes.APROBACION_PENDIENTE), Contar(PlaneaConstantes.APROBACION_APROBADA),
                Contar(PlaneaConstantes.APROBACION_DESCARTADA), ultima?.FechaRevision, ultima?.RevisadoPor);
        }

        public async Task<ResultadoAprobacionPlaneaDTO> AprobarAsync(
            int idPlanEstudios, int idPeriodo, IReadOnlyCollection<int> idsAprobados,
            string revisadoPor, CancellationToken cancellationToken = default)
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync(cancellationToken);

            // Solo las pendientes y enlazadas del plan × periodo; cualquier otro id se ignora (incluye el doble envío).
            var pendientes = await _context.ExperienciaEducativaPeriodo
                .Include(c => c.Horarios)
                .Include(c => c.Docentes)
                .Include(c => c.IdExperienciaEducativaNavigation)
                .Include(c => c.IdPlanEstudiosNavigation)
                .Where(c => c.IdPlanEstudios == idPlanEstudios && c.IdPeriodo == idPeriodo
                    && c.IdExperienciaEducativa != null
                    && c.EstadoAprobacion == PlaneaConstantes.APROBACION_PENDIENTE)
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

            var aprobar = idsAprobados.ToHashSet();
            var ahora = DateTime.UtcNow;

            var numerosPersonal = pendientes
                .Where(c => aprobar.Contains(c.IdExperienciaEducativaPeriodo))
                .SelectMany(c => c.Docentes)
                .Select(d => d.NumeroPersonal?.Trim())
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .ToList();

            var docentes = new Dictionary<string, int>();
            if (numerosPersonal.Count > 0)
            {
                var catalogo = await _context.Docente.AsNoTracking()
                    .Where(d => d.NumeroPersonal != null && numerosPersonal.Contains(d.NumeroPersonal))
                    .Select(d => new { d.IdDocente, NumeroPersonal = d.NumeroPersonal! })
                    .ToListAsync(cancellationToken);
                foreach (var d in catalogo)
                    docentes.TryAdd(d.NumeroPersonal.Trim(), d.IdDocente);
            }

            var nrcs = pendientes.Select(c => c.Nrc).Distinct().ToList();
            var existentes = await _context.Oferta.AsNoTracking()
                .Where(o => o.IdPeriodo == idPeriodo && nrcs.Contains(o.Nrc))
                .Select(o => new { o.IdOferta, o.Nrc, o.IdExperienciaEducativa })
                .ToListAsync(cancellationToken);

            // Una oferta ya enlazada a otra copia no se reutiliza (índice único sobre idOferta).
            var idsExistentes = existentes.Select(o => o.IdOferta).ToList();
            var yaEnlazadas = (await _context.ExperienciaEducativaPeriodo.AsNoTracking()
                .Where(c => c.IdOferta != null && idsExistentes.Contains(c.IdOferta.Value))
                .Select(c => c.IdOferta!.Value)
                .ToListAsync(cancellationToken)).ToHashSet();

            int creadas = 0, descartadas = 0, enlazadas = 0;

            foreach (var copia in pendientes)
            {
                copia.FechaRevision = ahora;
                copia.RevisadoPor = revisadoPor;

                if (!aprobar.Contains(copia.IdExperienciaEducativaPeriodo))
                {
                    copia.EstadoAprobacion = PlaneaConstantes.APROBACION_DESCARTADA;
                    descartadas++;
                    continue;
                }

                copia.EstadoAprobacion = PlaneaConstantes.APROBACION_APROBADA;

                var existente = existentes.FirstOrDefault(o =>
                    o.Nrc == copia.Nrc && o.IdExperienciaEducativa == copia.IdExperienciaEducativa
                    && !yaEnlazadas.Contains(o.IdOferta));
                if (existente is not null)
                {
                    copia.IdOferta = existente.IdOferta;
                    yaEnlazadas.Add(existente.IdOferta);
                    enlazadas++;
                    continue;
                }

                copia.IdOfertaNavigation = CrearOferta(copia, docentes, ahora);
                creadas++;
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);

            return new ResultadoAprobacionPlaneaDTO(creadas, descartadas, enlazadas);
        }

        public async Task<bool> RestaurarAsync(int idExperienciaEducativaPeriodo, CancellationToken cancellationToken = default)
        {
            var copia = await _context.ExperienciaEducativaPeriodo
                .FirstOrDefaultAsync(c => c.IdExperienciaEducativaPeriodo == idExperienciaEducativaPeriodo
                    && c.EstadoAprobacion == PlaneaConstantes.APROBACION_DESCARTADA, cancellationToken);

            if (copia is null) return false;

            copia.EstadoAprobacion = PlaneaConstantes.APROBACION_PENDIENTE;
            copia.FechaRevision = null;
            copia.RevisadoPor = null;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static Oferta CrearOferta(
            ExperienciaEducativaPeriodo copia, IReadOnlyDictionary<string, int> docentes, DateTime ahora)
        {
            var oferta = new Oferta
            {
                Nrc = copia.Nrc,
                IdExperienciaEducativa = copia.IdExperienciaEducativa!.Value,
                IdProgramaEducativo = copia.IdPlanEstudiosNavigation!.IdProgramaEducativo,
                IdPeriodo = copia.IdPeriodo,
                Hsm = ProgramacionPlaneaService.CalcularHsm(
                    copia.Horarios.Select(h => new HorarioPlaneaDTO(h.Dia, h.HoraInicio, h.HoraFin, h.Edificio, h.Aula)),
                    copia.IdExperienciaEducativaNavigation!.Horas)
            };

            // Solo cuentan los docentes que imparten; si hay varios se toma el primero.
            var docente = copia.Docentes
                .Where(d => d.Imparte != false)
                .OrderBy(d => d.IdExperienciaEducativaPeriodoDocente)
                .FirstOrDefault();

            if (docente is null)
            {
                oferta.Incluida = true;
            }
            else
            {
                var numero = docente.NumeroPersonal?.Trim();
                if (!string.IsNullOrEmpty(numero) && docentes.TryGetValue(numero, out var idDocente))
                {
                    oferta.IdDocente = idDocente;
                }
                else
                {
                    oferta.NumeroPersonalImportado = string.IsNullOrEmpty(numero) ? null : numero;
                    oferta.NombreDocenteImportado = docente.Nombre;
                }
                oferta.Incluida = false;
            }

            var vistos = new HashSet<(string, TimeOnly, TimeOnly, string?)>();
            foreach (var h in copia.Horarios.OrderBy(h => h.IdExperienciaEducativaPeriodoHorario))
            {
                var partes = new[] { h.Edificio, h.Aula }.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.Trim());
                var salon = string.Join("/", partes);
                var salonFinal = salon.Length == 0 ? null : salon;
                if (!vistos.Add((h.Dia, h.HoraInicio, h.HoraFin, salonFinal))) continue;

                oferta.Horario.Add(new Horario { Dia = h.Dia, HoraInicio = h.HoraInicio, HoraFin = h.HoraFin, Salon = salonFinal });
            }

            oferta.Log.Add(new Log { Mensaje = Constantes.HISTORIAL_APROBADA_PLANEA, Fecha = ahora });
            return oferta;
        }

        private sealed record FilaCopia(
            int IdExperienciaEducativaPeriodo, string EstadoAprobacion, string CodigoPeriodo, string Nrc,
            string CodigoExperiencia, string NombreExperiencia, string? CodigoPlan, string ProgramaEducativo,
            List<HorarioPlaneaDTO> Horarios, List<DocentePlaneaDTO> Docentes);

        private static async Task<List<CopiaProgramacionPlaneaDTO>> ProyectarAsync(
            IQueryable<ExperienciaEducativaPeriodo> consulta, CancellationToken cancellationToken)
        {
            var filas = await consulta
                .Select(c => new FilaCopia(
                    c.IdExperienciaEducativaPeriodo,
                    c.EstadoAprobacion,
                    c.IdPeriodoNavigation.Codigo.Trim(),
                    c.Nrc,
                    c.IdExperienciaEducativaNavigation!.Codigo,
                    c.IdExperienciaEducativaNavigation.Nombre,
                    c.IdPlanEstudiosNavigation!.CodigoPlan,
                    c.IdPlanEstudiosNavigation.IdProgramaEducativoNavigation.Nombre,
                    c.Horarios
                        .Select(h => new HorarioPlaneaDTO(h.Dia, h.HoraInicio, h.HoraFin, h.Edificio, h.Aula))
                        .ToList(),
                    c.Docentes
                        .OrderBy(d => d.IdExperienciaEducativaPeriodoDocente)
                        .Select(d => new DocentePlaneaDTO(d.Nombre, d.Imparte))
                        .ToList()))
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

            return filas
                .Select(c => new CopiaProgramacionPlaneaDTO(
                    c.CodigoPeriodo, c.Nrc, c.CodigoExperiencia, c.NombreExperiencia, c.CodigoPlan,
                    c.ProgramaEducativo,
                    c.Horarios
                        .OrderBy(h => IndiceDia(h.Dia))
                        .ThenBy(h => h.HoraInicio)
                        .ToList(),
                    c.Docentes,
                    c.IdExperienciaEducativaPeriodo, c.EstadoAprobacion))
                .ToList();
        }

        public async Task<EncabezadoProgramacionPlaneaDTO?> ObtenerEncabezadoAsync(
            int idPlanEstudios, int idPeriodo, CancellationToken cancellationToken = default)
        {
            var codigoPeriodo = await _context.Periodo.AsNoTracking()
                .Where(p => p.IdPeriodo == idPeriodo)
                .Select(p => p.Codigo.Trim())
                .FirstOrDefaultAsync(cancellationToken);

            if (codigoPeriodo is null)
                return null;

            return await _context.PlanEstudios.AsNoTracking()
                .Where(pl => pl.IdPlanEstudios == idPlanEstudios)
                .Select(pl => new EncabezadoProgramacionPlaneaDTO(
                    pl.IdProgramaEducativoNavigation.IdEntidadAcademica,
                    pl.IdProgramaEducativoNavigation.IdEntidadAcademicaNavigation.Nombre,
                    pl.IdProgramaEducativoNavigation.IdEntidadAcademicaNavigation.Region,
                    pl.IdProgramaEducativoNavigation.Nombre, pl.CodigoPlan, codigoPeriodo, ""))
                .FirstOrDefaultAsync(cancellationToken);
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
