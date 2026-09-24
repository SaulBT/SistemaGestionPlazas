using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.Catalogos;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

public sealed class NormalizedRegionCampusMvcRepository : IRegionCampusMvcRepository
{
    private readonly SgplaDbContext _db;

    public NormalizedRegionCampusMvcRepository(SgplaDbContext db) => _db = db;

    public async Task<IReadOnlyList<RegionAdministracionFila>> ListarRegionesAsync(CancellationToken cancellationToken = default) =>
        await _db.Regiones.AsNoTracking().Where(x => x.FechaEliminacion == null).OrderBy(x => x.Clave)
            .Select(x => new RegionAdministracionFila(x.Id, x.Clave, x.Nombre)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CampusAdministracionFila>> ListarCampusAsync(CancellationToken cancellationToken = default) =>
        await (from campus in _db.Campuses.AsNoTracking()
               join region in _db.Regiones.AsNoTracking() on campus.RegionId equals region.Id
               where campus.FechaEliminacion == null && region.FechaEliminacion == null
               orderby region.Clave, campus.Nombre
               select new CampusAdministracionFila(campus.Id, campus.Clave, campus.Nombre, region.Id, region.Nombre))
            .ToListAsync(cancellationToken);

    public Task<bool> RegionActivaAsync(int regionId, CancellationToken cancellationToken = default) =>
        _db.Regiones.AsNoTracking().AnyAsync(x => x.Id == regionId && x.FechaEliminacion == null, cancellationToken);

    public async Task CrearRegionAsync(CrearRegionMvcDto dto, CancellationToken cancellationToken = default)
    {
        var region = new Region { Clave = dto.Clave, Nombre = dto.Nombre.Trim() };
        _db.Entry(region).State = EntityState.Added;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> EditarNombreRegionAsync(int id, string nombre, CancellationToken cancellationToken = default)
    {
        var region = await _db.Regiones.AsTracking().FirstOrDefaultAsync(x => x.Id == id && x.FechaEliminacion == null, cancellationToken);
        if (region is null) return false;
        region.Nombre = nombre.Trim();
        _db.Entry(region).State = EntityState.Modified;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DarDeBajaRegionAsync(int id, DateTime instanteUtc, CancellationToken cancellationToken = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        var region = _db.Regiones.Where(x => x.Id == id && x.FechaEliminacion == null);
        if (!await region.AnyAsync(cancellationToken)) return false;
        if (await _db.Campuses.AsNoTracking().AnyAsync(x => x.RegionId == id && x.FechaEliminacion == null, cancellationToken))
            throw new InvalidOperationException("No se puede dar de baja una región que tiene Campus vigentes.");
        await region.ExecuteUpdateAsync(set => set.SetProperty(x => x.FechaEliminacion, instanteUtc), cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return true;
    }

    public async Task CrearCampusAsync(CrearCampusMvcDto dto, CancellationToken cancellationToken = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        if (!await RegionActivaAsync(dto.RegionId, cancellationToken))
            throw new ArgumentException("La región seleccionada ya no está vigente.");
        var campus = new Campus { Clave = dto.Clave.Trim().ToUpperInvariant(), Nombre = dto.Nombre.Trim(), RegionId = dto.RegionId };
        _db.Entry(campus).State = EntityState.Added;
        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    public async Task<bool> EditarNombreCampusAsync(int id, string nombre, CancellationToken cancellationToken = default)
    {
        var campus = await _db.Campuses.AsTracking().FirstOrDefaultAsync(x => x.Id == id && x.FechaEliminacion == null, cancellationToken);
        if (campus is null) return false;
        campus.Nombre = nombre.Trim();
        _db.Entry(campus).State = EntityState.Modified;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DarDeBajaCampusAsync(int id, DateTime instanteUtc, CancellationToken cancellationToken = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        var campus = _db.Campuses.Where(x => x.Id == id && x.FechaEliminacion == null);
        if (!await campus.AnyAsync(cancellationToken)) return false;
        if (await _db.EntidadAcademicas.AsNoTracking().AnyAsync(x => x.CampusId == id && x.FechaEliminacion == null, cancellationToken))
            throw new InvalidOperationException("No se puede dar de baja un Campus que tiene Entidades Académicas vigentes.");
        await campus.ExecuteUpdateAsync(set => set.SetProperty(x => x.FechaEliminacion, instanteUtc), cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return true;
    }
}
