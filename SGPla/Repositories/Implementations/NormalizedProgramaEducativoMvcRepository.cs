using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

public sealed class NormalizedProgramaEducativoMvcRepository : IProgramaEducativoMvcRepository
{
    private readonly SgplaDbContext _db;

    public NormalizedProgramaEducativoMvcRepository(SgplaDbContext db) => _db = db;

    private IQueryable<ProgramaEducativoMvcDto> ConsultaActivos(ProgramaEducativoMvcFiltro? filtro = null, int? id = null)
    {
        var query =
            from programa in _db.ProgramasEducativos.AsNoTracking()
            join entidad in _db.EntidadAcademicas.AsNoTracking() on programa.EntidadAcademicaId equals entidad.Id
            join area in _db.AreaAcademicas.AsNoTracking() on entidad.AreaAcademicaId equals area.Id
            join campus in _db.Campuses.AsNoTracking() on entidad.CampusId equals campus.Id
            join region in _db.Regiones.AsNoTracking() on campus.RegionId equals region.Id
            join sistema in _db.SistemasEducativos.AsNoTracking() on programa.SistemaEducativoId equals sistema.Id
            join nivel in _db.NivelesFormacion.AsNoTracking() on programa.NivelFormacionId equals nivel.Id
            where programa.FechaEliminacion == null && entidad.FechaEliminacion == null &&
                  area.FechaEliminacion == null && campus.FechaEliminacion == null && region.FechaEliminacion == null &&
                  sistema.FechaEliminacion == null && nivel.FechaEliminacion == null
            orderby programa.Nombre, programa.Id
            select new { programa, entidad, area, campus, region, sistema, nivel };

        if (id.HasValue) query = query.Where(x => x.programa.Id == id.Value);
        if (filtro is not null)
        {
            if (!string.IsNullOrWhiteSpace(filtro.Nombre)) query = query.Where(x => x.programa.Nombre.Contains(filtro.Nombre.Trim()));
            if (filtro.RegionId.HasValue) query = query.Where(x => x.region.Id == filtro.RegionId.Value);
            if (filtro.AreaAcademicaId.HasValue) query = query.Where(x => x.area.Id == filtro.AreaAcademicaId.Value);
            if (filtro.EntidadAcademicaId.HasValue) query = query.Where(x => x.entidad.Id == filtro.EntidadAcademicaId.Value);
        }
        return query.Select(x => new ProgramaEducativoMvcDto(x.programa.Id, x.programa.Nombre,
            x.entidad.Id, x.entidad.Nombre, x.area.Id, x.area.Nombre, x.campus.Id, x.campus.Nombre,
            x.region.Id, x.region.Nombre, x.sistema.Id, x.sistema.Nombre, x.nivel.Id, x.nivel.Nombre));
    }

    public async Task<PaginaProgramasEducativosMvc> BuscarAsync(ProgramaEducativoMvcFiltro filtro, CancellationToken cancellationToken = default)
    {
        var query = ConsultaActivos(filtro);
        var total = await query.CountAsync(cancellationToken);
        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.TamanoPagina, 1, 100);
        var items = await query.Skip((pagina - 1) * tamano).Take(tamano).ToListAsync(cancellationToken);
        return new PaginaProgramasEducativosMvc(items, total);
    }

    public Task<ProgramaEducativoMvcDto?> ObtenerAsync(int id, CancellationToken cancellationToken = default) =>
        ConsultaActivos(id: id).FirstOrDefaultAsync(cancellationToken);

    public async Task<int> CrearAsync(GuardarProgramaEducativoMvcDto dto, CancellationToken cancellationToken = default)
    {
        var entidad = new ProgramaEducativo { Nombre = dto.Nombre.Trim(), EntidadAcademicaId = dto.EntidadAcademicaId,
            SistemaEducativoId = dto.SistemaEducativoId, NivelFormacionId = dto.NivelFormacionId };
        _db.Entry(entidad).State = EntityState.Added;
        await _db.SaveChangesAsync(cancellationToken);
        return entidad.Id;
    }

    public async Task<bool> ActualizarAsync(GuardarProgramaEducativoMvcDto dto, CancellationToken cancellationToken = default)
    {
        var entidad = await _db.ProgramasEducativos.AsTracking()
            .FirstOrDefaultAsync(x => x.Id == dto.Id && x.FechaEliminacion == null, cancellationToken);
        if (entidad is null) return false;
        entidad.Nombre = dto.Nombre.Trim();
        entidad.EntidadAcademicaId = dto.EntidadAcademicaId;
        entidad.SistemaEducativoId = dto.SistemaEducativoId;
        entidad.NivelFormacionId = dto.NivelFormacionId;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    // Incluye planes dados de baja: el esquema permite cambiar sistema/nivel
    // únicamente si el programa nunca tuvo un plan.
    public Task<bool> TienePlanesAsync(int programaEducativoId, CancellationToken cancellationToken = default) =>
        _db.PlanesEstudios.AsNoTracking()
            .AnyAsync(x => x.ProgramaEducativoId == programaEducativoId, cancellationToken);

    public async Task<bool> EliminarAsync(int id, DateTime instanteUtc, CancellationToken cancellationToken = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        var programa = _db.ProgramasEducativos.Where(x => x.Id == id && x.FechaEliminacion == null);
        if (!await programa.AnyAsync(cancellationToken)) return false;

        var planes = _db.PlanesEstudios.Where(x => x.ProgramaEducativoId == id && x.FechaEliminacion == null);
        var planIds = planes.Select(x => x.Id);
        await _db.ExperienciasEducativas.Where(x => planIds.Contains(x.PlanEstudiosId) && x.FechaEliminacion == null)
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.FechaEliminacion, instanteUtc), cancellationToken);
        await planes.ExecuteUpdateAsync(set => set.SetProperty(x => x.FechaEliminacion, instanteUtc), cancellationToken);
        await programa.ExecuteUpdateAsync(set => set.SetProperty(x => x.FechaEliminacion, instanteUtc), cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> CatalogosActivosAsync(int entidadAcademicaId, int sistemaEducativoId, int nivelFormacionId, CancellationToken cancellationToken = default) =>
        await _db.EntidadAcademicas.AsNoTracking().AnyAsync(x => x.Id == entidadAcademicaId && x.FechaEliminacion == null &&
            _db.Campuses.Any(c => c.Id == x.CampusId && c.FechaEliminacion == null && _db.Regiones.Any(r => r.Id == c.RegionId && r.FechaEliminacion == null)) &&
            _db.AreaAcademicas.Any(a => a.Id == x.AreaAcademicaId && a.FechaEliminacion == null), cancellationToken) &&
        await _db.SistemasEducativos.AsNoTracking().AnyAsync(x => x.Id == sistemaEducativoId && x.FechaEliminacion == null, cancellationToken) &&
        await _db.NivelesFormacion.AsNoTracking().AnyAsync(x => x.Id == nivelFormacionId && x.FechaEliminacion == null, cancellationToken);
}
