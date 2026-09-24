using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

public sealed class NormalizedPlanEstudiosMvcRepository : IPlanEstudiosMvcRepository
{
    private readonly SgplaDbContext _db;

    public NormalizedPlanEstudiosMvcRepository(SgplaDbContext db) => _db = db;

    public async Task<PaginaPlanesEstudiosMvc> BuscarAsync(PlanEstudiosMvcFiltro filtro, CancellationToken cancellationToken = default)
    {
        var query = from plan in _db.PlanesEstudios.AsNoTracking()
               join programa in _db.ProgramasEducativos.AsNoTracking() on plan.ProgramaEducativoId equals programa.Id
               join entidad in _db.EntidadAcademicas.AsNoTracking() on programa.EntidadAcademicaId equals entidad.Id
               join area in _db.AreaAcademicas.AsNoTracking() on entidad.AreaAcademicaId equals area.Id
               join campus in _db.Campuses.AsNoTracking() on entidad.CampusId equals campus.Id
               join region in _db.Regiones.AsNoTracking() on campus.RegionId equals region.Id
               join sistema in _db.SistemasEducativos.AsNoTracking() on programa.SistemaEducativoId equals sistema.Id
               join nivel in _db.NivelesFormacion.AsNoTracking() on programa.NivelFormacionId equals nivel.Id
               where plan.FechaEliminacion == null && programa.FechaEliminacion == null &&
                     entidad.FechaEliminacion == null && area.FechaEliminacion == null &&
                     campus.FechaEliminacion == null && region.FechaEliminacion == null &&
                     sistema.FechaEliminacion == null && nivel.FechaEliminacion == null
               select new { plan, programa, entidad, area, region, sistema, nivel };
        if (filtro.RegionId.HasValue) query = query.Where(x => x.region.Id == filtro.RegionId.Value);
        if (filtro.AreaAcademicaId.HasValue) query = query.Where(x => x.area.Id == filtro.AreaAcademicaId.Value);
        if (filtro.EntidadAcademicaId.HasValue) query = query.Where(x => x.entidad.Id == filtro.EntidadAcademicaId.Value);
        if (filtro.ProgramaEducativoId.HasValue) query = query.Where(x => x.programa.Id == filtro.ProgramaEducativoId.Value);
        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var busqueda = filtro.Busqueda.Trim();
            query = query.Where(x => x.plan.Codigo.Contains(busqueda) || x.programa.Nombre.Contains(busqueda));
        }

        var total = await query.CountAsync(cancellationToken);
        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.TamanoPagina, 1, 100);
        var items = await query.OrderBy(x => x.programa.Nombre).ThenBy(x => x.plan.Codigo).ThenBy(x => x.plan.Id)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .Select(x => new PlanEstudiosMvcFila(x.plan.Id, x.plan.Codigo, x.programa.Id, x.programa.Nombre,
                x.entidad.Id, x.entidad.Nombre, x.area.Id, x.area.Nombre, x.region.Id, x.region.Nombre,
                x.sistema.Nombre, x.nivel.Nombre)).ToListAsync(cancellationToken);
        return new PaginaPlanesEstudiosMvc(items, total);
    }

    public Task<PlanEstudiosMvcDetalle?> ObtenerAsync(int id, CancellationToken cancellationToken = default) =>
        (from plan in _db.PlanesEstudios.AsNoTracking()
         join programa in _db.ProgramasEducativos.AsNoTracking() on plan.ProgramaEducativoId equals programa.Id
         join entidad in _db.EntidadAcademicas.AsNoTracking() on programa.EntidadAcademicaId equals entidad.Id
         join area in _db.AreaAcademicas.AsNoTracking() on entidad.AreaAcademicaId equals area.Id
         join campus in _db.Campuses.AsNoTracking() on entidad.CampusId equals campus.Id
         join region in _db.Regiones.AsNoTracking() on campus.RegionId equals region.Id
         join sistema in _db.SistemasEducativos.AsNoTracking() on programa.SistemaEducativoId equals sistema.Id
         join nivel in _db.NivelesFormacion.AsNoTracking() on programa.NivelFormacionId equals nivel.Id
         where plan.Id == id && plan.FechaEliminacion == null && programa.FechaEliminacion == null &&
               entidad.FechaEliminacion == null && area.FechaEliminacion == null && campus.FechaEliminacion == null &&
               region.FechaEliminacion == null && sistema.FechaEliminacion == null && nivel.FechaEliminacion == null
         select new PlanEstudiosMvcDetalle(plan.Id, plan.Codigo, programa.Id, programa.Nombre,
             entidad.Id, entidad.Nombre, area.Nombre, region.Nombre, sistema.Nombre, nivel.Nombre))
        .FirstOrDefaultAsync(cancellationToken);

    public async Task<int> CrearAsync(GuardarPlanEstudiosMvcDto dto, CancellationToken cancellationToken = default)
    {
        var plan = new PlanEstudios { Codigo = dto.Codigo.Trim().ToUpperInvariant(), ProgramaEducativoId = dto.ProgramaEducativoId };
        _db.Entry(plan).State = EntityState.Added;
        await _db.SaveChangesAsync(cancellationToken);
        return plan.Id;
    }

    public Task<bool> ProgramaActivoAsync(int programaEducativoId, CancellationToken cancellationToken = default) =>
        (from programa in _db.ProgramasEducativos.AsNoTracking()
         join entidad in _db.EntidadAcademicas.AsNoTracking() on programa.EntidadAcademicaId equals entidad.Id
         join area in _db.AreaAcademicas.AsNoTracking() on entidad.AreaAcademicaId equals area.Id
         join campus in _db.Campuses.AsNoTracking() on entidad.CampusId equals campus.Id
         join region in _db.Regiones.AsNoTracking() on campus.RegionId equals region.Id
         join sistema in _db.SistemasEducativos.AsNoTracking() on programa.SistemaEducativoId equals sistema.Id
         join nivel in _db.NivelesFormacion.AsNoTracking() on programa.NivelFormacionId equals nivel.Id
         where programa.Id == programaEducativoId && programa.FechaEliminacion == null &&
               entidad.FechaEliminacion == null && area.FechaEliminacion == null && campus.FechaEliminacion == null &&
               region.FechaEliminacion == null && sistema.FechaEliminacion == null && nivel.FechaEliminacion == null
         select programa.Id).AnyAsync(cancellationToken);

    public async Task<bool> EliminarAsync(int id, DateTime instanteUtc, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var plan = _db.PlanesEstudios.Where(x => x.Id == id && x.FechaEliminacion == null);
        if (!await plan.AnyAsync(cancellationToken)) return false;

        var experienciaIds = _db.ExperienciasEducativas.AsNoTracking()
            .Where(x => x.PlanEstudiosId == id).Select(x => x.Id);
        var programaciones = _db.ProgramacionAcademicas
            .Where(x => experienciaIds.Contains(x.ExperienciaEducativaId));
        var programacionIds = programaciones.Select(x => x.Id);
        await _db.HorariosProgramacion.Where(x => programacionIds.Contains(x.ProgramacionAcademicaId))
            .ExecuteDeleteAsync(cancellationToken);
        await programaciones.Where(x => x.FechaEliminacion == null)
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.FechaEliminacion, instanteUtc), cancellationToken);
        await _db.ExperienciasEducativas.Where(x => experienciaIds.Contains(x.Id) && x.FechaEliminacion == null)
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.FechaEliminacion, instanteUtc), cancellationToken);
        await plan.ExecuteUpdateAsync(set => set.SetProperty(x => x.FechaEliminacion, instanteUtc), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<PaginaExperienciasEducativasMvc> BuscarExperienciasAsync(
        int planId, string? busqueda, int pagina, int tamanoPagina, CancellationToken cancellationToken = default)
    {
        var query = from experiencia in _db.ExperienciasEducativas.AsNoTracking()
                    join area in _db.AreaFormaciones.AsNoTracking() on experiencia.AreaFormacionId equals area.Id
                    where experiencia.PlanEstudiosId == planId && experiencia.FechaEliminacion == null
                    select new { experiencia, area };
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim();
            query = query.Where(x => x.experiencia.Nombre.Contains(texto) || x.experiencia.MateriaEe.Contains(texto) || x.experiencia.CursoEe.Contains(texto));
        }
        var total = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, pagina);
        var size = Math.Clamp(tamanoPagina, 1, 100);
        var items = await query.OrderBy(x => x.experiencia.MateriaEe).ThenBy(x => x.experiencia.CursoEe)
            .ThenBy(x => x.experiencia.Id).Skip((page - 1) * size).Take(size)
            .Select(x => new ExperienciaEducativaMvcDto(x.experiencia.Id, x.experiencia.Nombre,
                x.experiencia.MateriaEe, x.experiencia.CursoEe, x.experiencia.HorasTeoricas,
                x.experiencia.HorasPracticas, x.experiencia.Creditos, x.experiencia.PerfilDocente,
                x.area.Id, x.area.Nombre)).ToListAsync(cancellationToken);
        return new PaginaExperienciasEducativasMvc(items, total);
    }

    public Task<ExperienciaEducativaMvcDto?> ObtenerExperienciaAsync(int planId, int id, CancellationToken cancellationToken = default) =>
        (from experiencia in _db.ExperienciasEducativas.AsNoTracking()
         join area in _db.AreaFormaciones.AsNoTracking() on experiencia.AreaFormacionId equals area.Id
         where experiencia.Id == id && experiencia.PlanEstudiosId == planId &&
               experiencia.FechaEliminacion == null
         select new ExperienciaEducativaMvcDto(experiencia.Id, experiencia.Nombre, experiencia.MateriaEe,
             experiencia.CursoEe, experiencia.HorasTeoricas, experiencia.HorasPracticas, experiencia.Creditos,
             experiencia.PerfilDocente, area.Id, area.Nombre)).FirstOrDefaultAsync(cancellationToken);

    public async Task<int> CrearExperienciaAsync(GuardarExperienciaEducativaMvcDto dto, CancellationToken cancellationToken = default)
    {
        var experiencia = new ExperienciaEducativa
        {
            PlanEstudiosId = dto.PlanEstudiosId, Nombre = dto.Nombre.Trim(),
            MateriaEe = dto.MateriaEe.Trim().ToUpperInvariant(), CursoEe = dto.CursoEe.Trim().ToUpperInvariant(),
            HorasTeoricas = dto.HorasTeoricas, HorasPracticas = dto.HorasPracticas, Creditos = dto.Creditos,
            PerfilDocente = string.IsNullOrWhiteSpace(dto.PerfilDocente) ? null : dto.PerfilDocente.Trim(),
            AreaFormacionId = dto.AreaFormacionId
        };
        _db.Entry(experiencia).State = EntityState.Added;
        await _db.SaveChangesAsync(cancellationToken);
        return experiencia.Id;
    }

    public async Task<int> CrearExperienciasAsync(
        IReadOnlyList<GuardarExperienciaEducativaMvcDto> experiencias, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var entities = experiencias.Select(dto => new ExperienciaEducativa
        {
            PlanEstudiosId = dto.PlanEstudiosId,
            Nombre = dto.Nombre.Trim(),
            MateriaEe = dto.MateriaEe.Trim().ToUpperInvariant(),
            CursoEe = dto.CursoEe.Trim().ToUpperInvariant(),
            HorasTeoricas = dto.HorasTeoricas,
            HorasPracticas = dto.HorasPracticas,
            Creditos = dto.Creditos,
            PerfilDocente = string.IsNullOrWhiteSpace(dto.PerfilDocente) ? null : dto.PerfilDocente.Trim(),
            AreaFormacionId = dto.AreaFormacionId
        }).ToList();
        foreach (var entity in entities) _db.Entry(entity).State = EntityState.Added;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return entities.Count;
    }

    public async Task<bool> ActualizarExperienciaAsync(GuardarExperienciaEducativaMvcDto dto, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        var experiencia = await _db.ExperienciasEducativas.AsTracking().FirstOrDefaultAsync(
            x => x.Id == dto.Id && x.PlanEstudiosId == dto.PlanEstudiosId && x.FechaEliminacion == null, cancellationToken);
        if (experiencia is null) return false;
        var tieneProgramacion = await TieneProgramacionAsync(dto.Id, cancellationToken);
        if (tieneProgramacion &&
            (experiencia.HorasTeoricas != dto.HorasTeoricas ||
             experiencia.HorasPracticas != dto.HorasPracticas ||
             experiencia.Creditos != dto.Creditos || experiencia.AreaFormacionId != dto.AreaFormacionId))
            throw new InvalidOperationException("La Experiencia Educativa ya fue programada; solo pueden cambiar su nombre y perfil docente.");

        experiencia.Nombre = dto.Nombre.Trim();
        if (!tieneProgramacion)
        {
            experiencia.HorasTeoricas = dto.HorasTeoricas;
            experiencia.HorasPracticas = dto.HorasPracticas;
            experiencia.Creditos = dto.Creditos;
            experiencia.AreaFormacionId = dto.AreaFormacionId;
        }
        experiencia.PerfilDocente = string.IsNullOrWhiteSpace(dto.PerfilDocente) ? null : dto.PerfilDocente.Trim();
        _db.Entry(experiencia).State = EntityState.Modified;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public Task<bool> TieneProgramacionAsync(int experienciaEducativaId, CancellationToken cancellationToken = default) =>
        _db.ProgramacionAcademicas.AsNoTracking().AnyAsync(x => x.ExperienciaEducativaId == experienciaEducativaId, cancellationToken);

    public Task<bool> AreaFormacionActivaAsync(int id, CancellationToken cancellationToken = default) =>
        _db.AreaFormaciones.AsNoTracking().AnyAsync(x => x.Id == id && x.FechaEliminacion == null, cancellationToken);

    public async Task<bool> EliminarExperienciaAsync(int planId, int id, DateTime instanteUtc, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var query = _db.ExperienciasEducativas.Where(x => x.Id == id && x.PlanEstudiosId == planId && x.FechaEliminacion == null);
        if (!await query.AnyAsync(cancellationToken)) return false;
        var programaciones = _db.ProgramacionAcademicas.Where(x => x.ExperienciaEducativaId == id);
        var programacionIds = programaciones.Select(x => x.Id);
        await _db.HorariosProgramacion.Where(x => programacionIds.Contains(x.ProgramacionAcademicaId))
            .ExecuteDeleteAsync(cancellationToken);
        await programaciones.Where(x => x.FechaEliminacion == null)
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.FechaEliminacion, instanteUtc), cancellationToken);
        await query.ExecuteUpdateAsync(set => set.SetProperty(x => x.FechaEliminacion, instanteUtc), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }
}
