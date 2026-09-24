using System.Data;
using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.Integracion;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class SincronizacionPlaneaService : ISincronizacionPlaneaService
{
    private readonly SgplaDbContext _db;
    private readonly IPlaneaClient _planeaClient;
    private readonly IPlaneaSnapshotValidator _validator;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<SincronizacionPlaneaService> _logger;

    public SincronizacionPlaneaService(SgplaDbContext db, IPlaneaClient planeaClient,
        IPlaneaSnapshotValidator validator, TimeProvider timeProvider,
        ILogger<SincronizacionPlaneaService> logger)
    {
        _db = db;
        _planeaClient = planeaClient;
        _validator = validator;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<int> SincronizarAsync(int periodoEscolarId, CancellationToken cancellationToken = default)
    {
        var periodo = await _db.PeriodosEscolares.AsNoTracking()
            .Where(x => x.Id == periodoEscolarId && x.FechaEliminacion == null)
            .Select(x => new { x.Id, x.Clave, x.FechaInicio, x.FechaFin })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException("El periodo seleccionado no existe o está dado de baja.");

        var inicio = AhoraUtcSegundo();
        var bitacora = new SincronizacionPlanea
        {
            PeriodoEscolarId = periodo.Id,
            Estado = "EN_PROCESO",
            IniciadaEn = inicio,
            FinalizadaEn = null,
            RegistrosRecibidos = 0,
            RegistrosIgnorados = 0,
            SesionesGeneradas = 0,
            DuplicadosDescartados = 0,
            Advertencias = 0,
            MensajeError = null
        };
        _db.Entry(bitacora).State = EntityState.Added;
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            _db.ChangeTracker.Clear();
            if (await _db.SincronizacionesPlanea.AsNoTracking().AnyAsync(x =>
                    x.PeriodoEscolarId == periodo.Id && x.Estado == "EN_PROCESO", CancellationToken.None))
                throw new InvalidOperationException("Ya hay una sincronización PLANEA en curso para este periodo.");
            throw;
        }

        try
        {
            var registros = await _planeaClient.ObtenerProgramacionesAsync(periodo.Clave, cancellationToken);
            await AplicarSnapshotAsync(periodo.Id, periodo.Clave, periodo.FechaInicio, periodo.FechaFin,
                bitacora, registros, cancellationToken);
            return bitacora.Id;
        }
        catch (Exception ex)
        {
            await MarcarFallidaAsync(bitacora.Id, ex);
            _logger.LogError(ex, "Falló la sincronización PLANEA para el periodo {PeriodoId}; el snapshot anterior se conservó.", periodo.Id);
            throw;
        }
    }

    private async Task AplicarSnapshotAsync(int periodoId, string clavePeriodo, DateOnly inicioPeriodo,
        DateOnly finPeriodo, SincronizacionPlanea bitacora, IReadOnlyList<PlaneaRegistro> registros,
        CancellationToken cancellationToken)
    {
        await using var transaccion = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var programaciones = await (from programacion in _db.ProgramacionAcademicas.AsNoTracking()
                                    join periodo in _db.PeriodosEscolares.AsNoTracking() on programacion.PeriodoEscolarId equals periodo.Id
                                    join ee in _db.ExperienciasEducativas.AsNoTracking() on programacion.ExperienciaEducativaId equals ee.Id
                                    join plan in _db.PlanesEstudios.AsNoTracking() on ee.PlanEstudiosId equals plan.Id
                                    join programa in _db.ProgramasEducativos.AsNoTracking() on plan.ProgramaEducativoId equals programa.Id
                                    join entidad in _db.EntidadAcademicas.AsNoTracking() on programa.EntidadAcademicaId equals entidad.Id
                                    join campus in _db.Campuses.AsNoTracking() on entidad.CampusId equals campus.Id
                                    join area in _db.AreaAcademicas.AsNoTracking() on entidad.AreaAcademicaId equals area.Id
                                    where programacion.PeriodoEscolarId == periodoId
                                          && programacion.FechaEliminacion == null && periodo.FechaEliminacion == null
                                          && ee.FechaEliminacion == null && plan.FechaEliminacion == null
                                          && programa.FechaEliminacion == null && entidad.FechaEliminacion == null
                                          && campus.FechaEliminacion == null && area.FechaEliminacion == null
                                    select new ProgramacionPlaneaReferencia(programacion.Id, programacion.Nrc))
            .ToListAsync(cancellationToken);
        var programacionesPorNrc = programaciones.ToDictionary(x => x.Nrc, x => x.Id, StringComparer.Ordinal);
        var snapshot = _validator.Validar(clavePeriodo, inicioPeriodo, finPeriodo, programacionesPorNrc, registros);
        var docentesSnapshot = snapshot.Docentes;
        var ofertas = await _db.Ofertas.AsNoTracking().Where(x => _db.ProgramacionAcademicas.Any(p =>
                p.Id == x.ProgramacionAcademicaId && p.PeriodoEscolarId == periodoId && p.FechaEliminacion == null))
            .Select(x => x.ProgramacionAcademicaId).Distinct().ToListAsync(cancellationToken);
        var asignaciones = await _db.AsignacionDocentes.AsTracking().Where(x => _db.ProgramacionAcademicas.Any(p =>
                p.Id == x.ProgramacionAcademicaId && p.PeriodoEscolarId == periodoId && p.FechaEliminacion == null))
            .ToListAsync(cancellationToken);

        var numerosPersonal = docentesSnapshot.Select(x => x.NumeroPersonal).Distinct(StringComparer.Ordinal).ToArray();
        var docentesPorNumero = new Dictionary<string, Docente>(StringComparer.Ordinal);
        foreach (var bloque in numerosPersonal.Chunk(1000))
        {
            var existentes = await _db.Docentes.AsTracking()
                .Where(x => x.NumPersonal != null && bloque.Contains(x.NumPersonal))
                .ToListAsync(cancellationToken);
            foreach (var docente in existentes)
            {
                if (docente.NumPersonal is not null) docentesPorNumero[docente.NumPersonal] = docente;
            }
        }
        var numerosPorDocenteId = new Dictionary<int, string?>();
        var idsDocentesAsignados = asignaciones.Select(x => x.DocenteId).Distinct().ToArray();
        foreach (var bloque in idsDocentesAsignados.Chunk(1000))
        {
            var existentes = await _db.Docentes.AsNoTracking().Where(x => bloque.Contains(x.Id))
                .Select(x => new { x.Id, x.NumPersonal }).ToListAsync(cancellationToken);
            foreach (var docente in existentes) numerosPorDocenteId[docente.Id] = docente.NumPersonal;
        }

        var idsConOferta = ofertas.ToHashSet();
        var asignacionesNuevas = new List<AsignacionDocente>();
        var discrepancias = 0;
        foreach (var programacionId in idsConOferta)
        {
            var docentePlanea = docentesSnapshot.SingleOrDefault(x => x.ProgramacionAcademicaId == programacionId);
            var local = BuscarAsignacionEnPeriodo(asignaciones, programacionId, inicioPeriodo, finPeriodo);
            var numLocal = local is null ? null : numerosPorDocenteId.GetValueOrDefault(local.DocenteId);
            if (!string.Equals(numLocal, docentePlanea?.NumeroPersonal, StringComparison.Ordinal)) discrepancias++;
        }

        var asignacionesLocalesSinOferta = asignaciones
            .Where(x => x.Origen == "SGPLA" && !idsConOferta.Contains(x.ProgramacionAcademicaId)).ToArray();
        foreach (var asignacionLocal in asignacionesLocalesSinOferta)
        {
            var docentePlanea = docentesSnapshot.SingleOrDefault(x => x.ProgramacionAcademicaId == asignacionLocal.ProgramacionAcademicaId);
            if (docentePlanea is null || !string.Equals(
                    numerosPorDocenteId.GetValueOrDefault(asignacionLocal.DocenteId),
                    docentePlanea.NumeroPersonal, StringComparison.Ordinal))
                discrepancias++;
        }

        var idsGestionLocal = asignacionesLocalesSinOferta.Select(x => x.ProgramacionAcademicaId).ToHashSet();
        var asignacionesPlaneaReemplazables = asignaciones
            .Where(x => x.Origen == "PLANEA" && !idsConOferta.Contains(x.ProgramacionAcademicaId)
                && !idsGestionLocal.Contains(x.ProgramacionAcademicaId)).ToArray();
        foreach (var asignacion in asignacionesPlaneaReemplazables)
            _db.Entry(asignacion).State = EntityState.Deleted;

        foreach (var registro in docentesSnapshot)
        {
            if (!docentesPorNumero.TryGetValue(registro.NumeroPersonal, out var docente))
            {
                docente = new Docente { Nombre = registro.Nombre, NumPersonal = registro.NumeroPersonal };
                _db.Entry(docente).State = EntityState.Added;
                docentesPorNumero.Add(registro.NumeroPersonal, docente);
            }
            else if (!string.Equals(docente.Nombre, registro.Nombre, StringComparison.Ordinal))
            {
                docente.Nombre = registro.Nombre;
                _db.Entry(docente).State = EntityState.Modified;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        foreach (var registro in docentesSnapshot)
        {
            if (idsConOferta.Contains(registro.ProgramacionAcademicaId) || idsGestionLocal.Contains(registro.ProgramacionAcademicaId))
                continue;
            var docente = docentesPorNumero[registro.NumeroPersonal];
            var asignacion = new AsignacionDocente
            {
                ProgramacionAcademicaId = registro.ProgramacionAcademicaId,
                DocenteId = docente.Id,
                Origen = "PLANEA",
                FechaInicio = registro.FechaInicio,
                FechaFin = registro.FechaFin,
                SincronizacionPlaneaId = bitacora.Id,
                ActaOfertaId = null
            };
            asignacionesNuevas.Add(asignacion);
            _db.Entry(asignacion).State = EntityState.Added;
        }

        await _db.HorariosProgramacion.Where(h => _db.ProgramacionAcademicas.Any(p =>
                p.Id == h.ProgramacionAcademicaId && p.PeriodoEscolarId == periodoId))
            .ExecuteDeleteAsync(cancellationToken);
        foreach (var sesion in snapshot.Sesiones)
        {
            var horario = new HorarioProgramacion
            {
                ProgramacionAcademicaId = sesion.ProgramacionAcademicaId,
                SincronizacionPlaneaId = bitacora.Id,
                DiaSemana = sesion.DiaSemana,
                HoraInicio = sesion.HoraInicio,
                HoraFin = sesion.HoraFin,
                FechaInicio = sesion.FechaInicio,
                FechaFin = sesion.FechaFin,
                Edificio = sesion.Edificio,
                Aula = sesion.Aula
            };
            _db.Entry(horario).State = EntityState.Added;
        }

        bitacora.Estado = "EXITOSA";
        bitacora.FinalizadaEn = Max(AhoraUtcSegundo(), bitacora.IniciadaEn);
        bitacora.RegistrosRecibidos = registros.Count;
        bitacora.RegistrosIgnorados = snapshot.RegistrosIgnorados;
        bitacora.SesionesGeneradas = snapshot.Sesiones.Count;
        bitacora.DuplicadosDescartados = snapshot.DuplicadosDescartados;
        bitacora.Advertencias = snapshot.Advertencias + discrepancias;
        bitacora.MensajeError = null;
        _db.Entry(bitacora).State = EntityState.Modified;
        await _db.SaveChangesAsync(cancellationToken);
        await transaccion.CommitAsync(cancellationToken);

        if (bitacora.Advertencias > 0)
            _logger.LogWarning("La sincronización PLANEA {SincronizacionId} terminó con {Advertencias} advertencias.",
                bitacora.Id, bitacora.Advertencias);
    }

    private async Task MarcarFallidaAsync(int sincronizacionId, Exception error)
    {
        try
        {
            _db.ChangeTracker.Clear();
            var bitacora = await _db.SincronizacionesPlanea.AsTracking()
                .SingleOrDefaultAsync(x => x.Id == sincronizacionId, CancellationToken.None);
            if (bitacora is null || bitacora.Estado != "EN_PROCESO") return;
            bitacora.Estado = "FALLIDA";
            bitacora.FinalizadaEn = Max(AhoraUtcSegundo(), bitacora.IniciadaEn);
            var mensaje = error is OperationCanceledException
                ? "La sincronización fue cancelada; no se reemplazó el snapshot vigente."
                : error.Message;
            bitacora.MensajeError = mensaje.Length <= 4000 ? mensaje : mensaje[..4000];
            _db.Entry(bitacora).State = EntityState.Modified;
            await _db.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception errorAlRegistrar)
        {
            _logger.LogError(errorAlRegistrar, "No se pudo cerrar como FALLIDA la sincronización PLANEA {SincronizacionId}.", sincronizacionId);
        }
    }

    private static AsignacionDocente? BuscarAsignacionEnPeriodo(IReadOnlyList<AsignacionDocente> asignaciones,
        int programacionId, DateOnly inicioPeriodo, DateOnly finPeriodo)
    {
        var vigentes = asignaciones.Where(x => x.ProgramacionAcademicaId == programacionId
            && x.FechaInicio <= finPeriodo && (!x.FechaFin.HasValue || x.FechaFin.Value >= inicioPeriodo)).ToArray();
        if (vigentes.Length > 1)
            throw new InvalidDataException("Existe más de una asignación docente traslapada para una programación.");
        return vigentes.SingleOrDefault();
    }

    private DateTime AhoraUtcSegundo()
    {
        var utc = _timeProvider.GetUtcNow().UtcDateTime;
        return utc.AddTicks(-(utc.Ticks % TimeSpan.TicksPerSecond));
    }

    private static DateTime Max(DateTime primero, DateTime segundo) => primero >= segundo ? primero : segundo;
}
