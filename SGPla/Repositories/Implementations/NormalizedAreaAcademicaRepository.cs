using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Models;
using SGPla.Repositories.Interfaces;
using LegacyAreaAcademica = SGPla.Models.AreaAcademica;

namespace SGPla.Repositories.Implementations;

/// <summary>
/// Adaptador MVC para que las pantallas de áreas académicas escriban en
/// academico.area_academica. Los módulos REST conservan su repositorio propio.
/// </summary>
public sealed class NormalizedAreaAcademicaRepository : IAreaAcademicaRepository
{
    private readonly SgplaDbContext _db;

    public NormalizedAreaAcademicaRepository(SgplaDbContext db) => _db = db;

    public async Task<List<LegacyAreaAcademica>> ObtenerTodosAsync() =>
        await Query().OrderBy(x => x.Nombre).ToListAsync();

    public async Task<List<LegacyAreaAcademica>> ObtenerTodosOpcionesAsync() =>
        await Query().OrderBy(x => x.Nombre).ToListAsync();

    public async Task<List<LegacyAreaAcademica>> ObtenerPorNombreAsync(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return [];
        var filtro = nombre.Trim();
        return await Query().Where(x => x.Nombre.Contains(filtro)).OrderBy(x => x.Nombre).ToListAsync();
    }

    public async Task<LegacyAreaAcademica?> ObtenerPorIdAsync(int idAreaAcademica) =>
        await Query().FirstOrDefaultAsync(x => x.IdAreaAcademica == idAreaAcademica);

    public Task<bool> ExistePorIdAsync(int idAreaAcademica) =>
        _db.AreaAcademicas.AsNoTracking().AnyAsync(x => x.Id == idAreaAcademica && x.FechaEliminacion == null);

    public async Task<LegacyAreaAcademica> CrearAsync(LegacyAreaAcademica areaAcademica)
    {
        ArgumentNullException.ThrowIfNull(areaAcademica);
        var siguienteClave = (await _db.AreaAcademicas.AsNoTracking().MaxAsync(x => (int?)x.Clave) ?? 0) + 1;
        var entity = new Data.NewModel.Entities.AreaAcademica
        {
            Clave = siguienteClave,
            Nombre = areaAcademica.Nombre.Trim()
        };
        _db.AreaAcademicas.Add(entity);
        await _db.SaveChangesAsync();
        return ToLegacy(entity);
    }

    public async Task ActualizarAsync(LegacyAreaAcademica areaAcademica)
    {
        ArgumentNullException.ThrowIfNull(areaAcademica);
        var entity = await _db.AreaAcademicas.FirstOrDefaultAsync(x => x.Id == areaAcademica.IdAreaAcademica && x.FechaEliminacion == null)
            ?? throw new KeyNotFoundException("El área académica no existe.");
        entity.Nombre = areaAcademica.Nombre.Trim();
        _db.Entry(entity).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    public async Task EliminarAsync(LegacyAreaAcademica areaAcademica)
    {
        ArgumentNullException.ThrowIfNull(areaAcademica);
        var entity = await _db.AreaAcademicas.FirstOrDefaultAsync(x => x.Id == areaAcademica.IdAreaAcademica && x.FechaEliminacion == null)
            ?? throw new KeyNotFoundException("El área académica no existe.");
        entity.FechaEliminacion = DateTime.UtcNow;
        _db.Entry(entity).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    public Task<int> ContarPorFiltroAsync(string busqueda) =>
        Query().CountAsync(x => string.IsNullOrWhiteSpace(busqueda) || x.Nombre.Contains(busqueda.Trim()));

    public async Task<List<LegacyAreaAcademica>> ObtenerPorFiltroAsync(string busqueda, int pagina, int cantidad) =>
        await Query().Where(x => string.IsNullOrWhiteSpace(busqueda) || x.Nombre.Contains(busqueda.Trim()))
            .OrderBy(x => x.Nombre).Skip(Math.Max(0, pagina - 1) * cantidad).Take(cantidad).ToListAsync();

    private IQueryable<LegacyAreaAcademica> Query() =>
        _db.AreaAcademicas.AsNoTracking().Where(x => x.FechaEliminacion == null).Select(ToLegacyExpression());

    private static System.Linq.Expressions.Expression<Func<SGPla.Data.NewModel.Entities.AreaAcademica, LegacyAreaAcademica>> ToLegacyExpression() =>
        x => new LegacyAreaAcademica
        {
            IdAreaAcademica = x.Id,
            Nombre = x.Nombre,
            Telefono = string.Empty,
            FechaEliminacion = x.FechaEliminacion
        };

    private static LegacyAreaAcademica ToLegacy(SGPla.Data.NewModel.Entities.AreaAcademica entity) => new()
    {
        IdAreaAcademica = entity.Id,
        Nombre = entity.Nombre,
        Telefono = string.Empty,
        FechaEliminacion = entity.FechaEliminacion
    };
}
