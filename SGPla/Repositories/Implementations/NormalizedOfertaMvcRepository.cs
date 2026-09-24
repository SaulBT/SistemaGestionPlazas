using System.Data;
using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.Ofertas;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

public sealed class NormalizedOfertaMvcRepository : IOfertaMvcRepository
{
    private readonly SgplaDbContext _db;
    private readonly TimeProvider _timeProvider;

    public NormalizedOfertaMvcRepository(SgplaDbContext db, TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
    }

    public async Task<OfertaMvcNuevaDatos?> ObtenerDatosNuevaAsync(
        int programacionAcademicaId, int entidadAcademicaId, CancellationToken cancellationToken = default)
    {
        var programacion = await ConsultaProgramacion(programacionAcademicaId, entidadAcademicaId)
            .Select(x => new
            {
                x.programacion.Id,
                x.programacion.Nrc,
                EntidadId = x.entidad.Id,
                Entidad = x.entidad.Nombre,
                Programa = x.programa.Nombre,
                Experiencia = x.ee.Nombre
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (programacion is null) return null;

        var hoy = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        var tieneDocente = await (from asignacion in _db.AsignacionDocentes.AsNoTracking()
                                  where asignacion.ProgramacionAcademicaId == programacion.Id
                                        && asignacion.FechaInicio <= hoy
                                        && (!asignacion.FechaFin.HasValue || asignacion.FechaFin >= hoy)
                                  select asignacion.Id).AnyAsync(cancellationToken);
        return new OfertaMvcNuevaDatos(
            new ProgramacionOfertaMvc(programacion.Id, programacion.Nrc, programacion.EntidadId,
                programacion.Entidad, programacion.Programa, programacion.Experiencia, tieneDocente),
            await _db.TipoPlazas.AsNoTracking().OrderBy(x => x.Nombre)
                .Select(x => new OfertaMvcOpcion(x.Id, x.Nombre)).ToListAsync(cancellationToken),
            await _db.TiposContratacion.AsNoTracking().OrderBy(x => x.Nombre)
                .Select(x => new OfertaMvcOpcion(x.Id, x.Nombre)).ToListAsync(cancellationToken));
    }

    public async Task<int> CrearAsync(CrearOfertaMvcDatos datos, int entidadAcademicaId,
        CancellationToken cancellationToken = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var programacion = await ConsultaProgramacion(datos.ProgramacionAcademicaId, entidadAcademicaId)
            .Select(x => x.programacion.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (programacion == 0)
            throw new ArgumentException("La programación no está vigente o no pertenece a tu Entidad Académica.");

        var tipoPlazaVigente = await _db.TipoPlazas.AsNoTracking().AnyAsync(x => x.Id == datos.TipoPlazaId, cancellationToken);
        var tipoContratacionVigente = await _db.TiposContratacion.AsNoTracking().AnyAsync(x => x.Id == datos.TipoContratacionId, cancellationToken);
        if (!tipoPlazaVigente || !tipoContratacionVigente)
            throw new ArgumentException("Selecciona un tipo de plaza y contratación vigentes.");

        var clave = datos.ClavePlaza.Trim().ToUpperInvariant();
        var abierta = await _db.Ofertas.AsNoTracking().AnyAsync(x =>
            x.ProgramacionAcademicaId == programacion && x.ClavePlaza == clave && x.CerradaEn == null, cancellationToken);
        if (abierta) throw new InvalidOperationException("Ya existe una Oferta abierta con esa clave para esta programación.");

        var hoy = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        var tieneDocente = await _db.AsignacionDocentes.AsNoTracking().AnyAsync(x =>
            x.ProgramacionAcademicaId == programacion && x.FechaInicio <= hoy
            && (!x.FechaFin.HasValue || x.FechaFin >= hoy), cancellationToken);
        var justificacion = string.IsNullOrWhiteSpace(datos.Justificacion) ? null : datos.Justificacion.Trim();
        if (tieneDocente && justificacion is null)
            throw new ArgumentException("La justificación es obligatoria cuando existe un docente vigente.");

        var oferta = new Oferta
        {
            ProgramacionAcademicaId = programacion,
            ClavePlaza = clave,
            TipoPlazaId = datos.TipoPlazaId,
            TipoContratacionId = datos.TipoContratacionId,
            PerfilSolicitado = datos.PerfilSolicitado.Trim(),
            Justificacion = justificacion,
            Estado = "DISPONIBLE",
            CerradaEn = null
        };
        _db.Entry(oferta).State = EntityState.Added;
        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return oferta.Id;
    }

    private IQueryable<ProgramacionOfertaProjection> ConsultaProgramacion(int programacionAcademicaId, int entidadAcademicaId) =>
        from programacion in _db.ProgramacionAcademicas.AsNoTracking()
        join periodo in _db.PeriodosEscolares.AsNoTracking() on programacion.PeriodoEscolarId equals periodo.Id
        join ee in _db.ExperienciasEducativas.AsNoTracking() on programacion.ExperienciaEducativaId equals ee.Id
        join plan in _db.PlanesEstudios.AsNoTracking() on ee.PlanEstudiosId equals plan.Id
        join programa in _db.ProgramasEducativos.AsNoTracking() on plan.ProgramaEducativoId equals programa.Id
        join entidad in _db.EntidadAcademicas.AsNoTracking() on programa.EntidadAcademicaId equals entidad.Id
        where programacion.Id == programacionAcademicaId && entidad.Id == entidadAcademicaId
              && programacion.FechaEliminacion == null && periodo.FechaEliminacion == null
              && ee.FechaEliminacion == null && plan.FechaEliminacion == null
              && programa.FechaEliminacion == null && entidad.FechaEliminacion == null
        select new ProgramacionOfertaProjection(programacion, ee, programa, entidad);

    private sealed record ProgramacionOfertaProjection(ProgramacionAcademica programacion,
        ExperienciaEducativa ee, ProgramaEducativo programa, EntidadAcademica entidad);
}
