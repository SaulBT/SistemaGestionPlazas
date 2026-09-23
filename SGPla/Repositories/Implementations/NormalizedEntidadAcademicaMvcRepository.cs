using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

public sealed class NormalizedEntidadAcademicaMvcRepository : IEntidadAcademicaMvcRepository
{
    private readonly SgplaDbContext _db;

    public NormalizedEntidadAcademicaMvcRepository(SgplaDbContext db) => _db = db;

    public async Task<(IReadOnlyList<EntidadAcademicaMvcDto> Items, int TotalCount)> BuscarAsync(
        FiltroEntidadAcademicaMvcDto filtro, CancellationToken cancellationToken = default)
    {
        var query =
            from entidad in _db.EntidadAcademicas.AsNoTracking()
            join campus in _db.Campuses.AsNoTracking() on entidad.CampusId equals campus.Id
            join region in _db.Regiones.AsNoTracking() on campus.RegionId equals region.Id
            join area in _db.AreaAcademicas.AsNoTracking() on entidad.AreaAcademicaId equals area.Id
            join municipio in _db.Municipios.AsNoTracking() on entidad.MunicipioId equals municipio.Id
            where entidad.FechaEliminacion == null && campus.FechaEliminacion == null &&
                  region.FechaEliminacion == null && area.FechaEliminacion == null
            select new { Entidad = entidad, Campus = campus, Region = region, Area = area, Municipio = municipio };
        if (filtro.RegionId.HasValue)
            query = query.Where(x => x.Region.Id == filtro.RegionId.Value);
        if (filtro.AreaAcademicaId.HasValue)
            query = query.Where(x => x.Entidad.AreaAcademicaId == filtro.AreaAcademicaId.Value);
        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var term = filtro.Busqueda.Trim();
            query = query.Where(x => x.Entidad.Nombre.Contains(term) || x.Entidad.Clave.Contains(term));
        }

        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Entidad.Nombre)
            .ThenBy(x => x.Entidad.Id)
            .Skip((filtro.Pagina - 1) * filtro.Cantidad)
            .Take(filtro.Cantidad)
            .Select(x => new EntidadAcademicaMvcDto(
                x.Entidad.Id, x.Entidad.Clave, x.Entidad.Nombre, x.Entidad.Calle,
                x.Entidad.NumeroExterior, x.Entidad.Colonia, x.Entidad.CodigoPostal,
                x.Entidad.Telefono, x.Entidad.Extension, x.Campus.Id, x.Campus.Nombre,
                x.Region.Id, x.Region.Nombre, x.Area.Id, x.Area.Nombre,
                x.Municipio.Id, x.Municipio.Nombre))
            .ToListAsync(cancellationToken);
        return (items, count);
    }

    public Task<EntidadAcademicaMvcDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        QueryActivas(id).FirstOrDefaultAsync(cancellationToken);

    public async Task<bool> ReferenciasActivasAsync(EntidadAcademicaMvcInputDto datos, CancellationToken cancellationToken = default)
    {
        var campusValido = await _db.Campuses.AsNoTracking().AnyAsync(c =>
            c.Id == datos.CampusId && c.FechaEliminacion == null &&
            _db.Regiones.Any(r => r.Id == c.RegionId && r.FechaEliminacion == null), cancellationToken);
        if (!campusValido) return false;

        var areaValida = await _db.AreaAcademicas.AsNoTracking()
            .AnyAsync(a => a.Id == datos.AreaAcademicaId && a.FechaEliminacion == null, cancellationToken);
        if (!areaValida) return false;

        return await _db.Municipios.AsNoTracking().AnyAsync(m => m.Id == datos.MunicipioId, cancellationToken);
    }

    public Task<bool> TieneProgramasAsync(int entidadId, CancellationToken cancellationToken = default) =>
        _db.ProgramasEducativos.AsNoTracking().AnyAsync(x => x.EntidadAcademicaId == entidadId, cancellationToken);

    public Task<bool> TieneUsuariosActivosAsync(int entidadId, CancellationToken cancellationToken = default) =>
        (from perfil in _db.UsuariosEntidadAcademica.AsNoTracking()
         join usuario in _db.Usuarios.AsNoTracking() on perfil.UsuarioId equals usuario.Id
         where perfil.EntidadAcademicaId == entidadId && usuario.FechaEliminacion == null
         select usuario.Id).AnyAsync(cancellationToken);

    public async Task<int> CrearAsync(EntidadAcademicaMvcInputDto datos, CancellationToken cancellationToken = default)
    {
        var entidad = new EntidadAcademica
        {
            Clave = datos.Clave.Trim().ToUpperInvariant(),
            Nombre = datos.Nombre.Trim(),
            Calle = datos.Calle.Trim(),
            NumeroExterior = NormalizarOpcional(datos.NumeroExterior),
            Colonia = datos.Colonia.Trim(),
            CodigoPostal = datos.CodigoPostal.Trim(),
            Telefono = datos.Telefono.Trim(),
            Extension = NormalizarOpcional(datos.Extension),
            CampusId = datos.CampusId,
            AreaAcademicaId = datos.AreaAcademicaId,
            MunicipioId = datos.MunicipioId
        };

        _db.EntidadAcademicas.Add(entidad);
        await _db.SaveChangesAsync(cancellationToken);
        return entidad.Id;
    }

    public async Task ActualizarAsync(int id, EntidadAcademicaMvcInputDto datos, CancellationToken cancellationToken = default)
    {
        var entidad = await _db.EntidadAcademicas.AsTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.FechaEliminacion == null, cancellationToken)
            ?? throw new KeyNotFoundException("La entidad académica no existe o está inactiva.");

        // Clave y campus son inmutables según DATABASE.md.
        entidad.Nombre = datos.Nombre.Trim();
        entidad.Calle = datos.Calle.Trim();
        entidad.NumeroExterior = NormalizarOpcional(datos.NumeroExterior);
        entidad.Colonia = datos.Colonia.Trim();
        entidad.CodigoPostal = datos.CodigoPostal.Trim();
        entidad.Telefono = datos.Telefono.Trim();
        entidad.Extension = NormalizarOpcional(datos.Extension);
        entidad.AreaAcademicaId = datos.AreaAcademicaId;
        entidad.MunicipioId = datos.MunicipioId;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task EliminarEnCascadaAsync(int id, DateTime fechaEliminacionUtc, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var entidadExiste = await _db.EntidadAcademicas.AsNoTracking()
            .AnyAsync(x => x.Id == id && x.FechaEliminacion == null, cancellationToken);
        if (!entidadExiste)
            throw new KeyNotFoundException("La entidad académica no existe o ya está inactiva.");

        if (await TieneUsuariosActivosAsync(id, cancellationToken))
            throw new InvalidOperationException("No se puede dar de baja la entidad porque tiene usuarios activos.");

        var programaIds = await _db.ProgramasEducativos.AsNoTracking()
            .Where(x => x.EntidadAcademicaId == id && x.FechaEliminacion == null)
            .Select(x => x.Id).ToListAsync(cancellationToken);
        var planIds = await _db.PlanesEstudios.AsNoTracking()
            .Where(x => programaIds.Contains(x.ProgramaEducativoId) && x.FechaEliminacion == null)
            .Select(x => x.Id).ToListAsync(cancellationToken);
        var experienciaIds = await _db.ExperienciasEducativas.AsNoTracking()
            .Where(x => planIds.Contains(x.PlanEstudiosId) && x.FechaEliminacion == null)
            .Select(x => x.Id).ToListAsync(cancellationToken);
        var programacionIds = await _db.ProgramacionAcademicas.AsNoTracking()
            .Where(x => experienciaIds.Contains(x.ExperienciaEducativaId) && x.FechaEliminacion == null)
            .Select(x => x.Id).ToListAsync(cancellationToken);

        if (programacionIds.Count > 0)
            await _db.HorariosProgramacion.Where(x => programacionIds.Contains(x.ProgramacionAcademicaId))
                .ExecuteDeleteAsync(cancellationToken);
        if (programacionIds.Count > 0)
            await _db.ProgramacionAcademicas.Where(x => programacionIds.Contains(x.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.FechaEliminacion, fechaEliminacionUtc), cancellationToken);
        if (experienciaIds.Count > 0)
            await _db.ExperienciasEducativas.Where(x => experienciaIds.Contains(x.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.FechaEliminacion, fechaEliminacionUtc), cancellationToken);
        if (planIds.Count > 0)
            await _db.PlanesEstudios.Where(x => planIds.Contains(x.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.FechaEliminacion, fechaEliminacionUtc), cancellationToken);
        if (programaIds.Count > 0)
            await _db.ProgramasEducativos.Where(x => programaIds.Contains(x.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.FechaEliminacion, fechaEliminacionUtc), cancellationToken);

        await _db.EntidadAcademicas.Where(x => x.Id == id && x.FechaEliminacion == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.FechaEliminacion, fechaEliminacionUtc), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private IQueryable<EntidadAcademicaMvcDto> QueryActivas(int? entidadId = null) =>
        from entidad in _db.EntidadAcademicas.AsNoTracking()
        join campus in _db.Campuses.AsNoTracking() on entidad.CampusId equals campus.Id
        join region in _db.Regiones.AsNoTracking() on campus.RegionId equals region.Id
        join area in _db.AreaAcademicas.AsNoTracking() on entidad.AreaAcademicaId equals area.Id
        join municipio in _db.Municipios.AsNoTracking() on entidad.MunicipioId equals municipio.Id
        where entidad.FechaEliminacion == null && campus.FechaEliminacion == null &&
              region.FechaEliminacion == null && area.FechaEliminacion == null &&
              (!entidadId.HasValue || entidad.Id == entidadId.Value)
        select new EntidadAcademicaMvcDto(
            entidad.Id, entidad.Clave, entidad.Nombre, entidad.Calle, entidad.NumeroExterior,
            entidad.Colonia, entidad.CodigoPostal, entidad.Telefono, entidad.Extension,
            campus.Id, campus.Nombre, region.Id, region.Nombre, area.Id, area.Nombre, municipio.Id, municipio.Nombre);

    private static string? NormalizarOpcional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
