using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Models;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

/// <summary>Repositorio MVC de artículos contra plazas.articulo.</summary>
public sealed class NormalizedArticuloRepository : IArticuloRepository
{
    private readonly SgplaDbContext _db;

    public NormalizedArticuloRepository(SgplaDbContext db) => _db = db;

    public async Task<IEnumerable<Articulo>> ObtenerTodosAsync() =>
        await Query().OrderBy(x => x.Numero).ToListAsync();

    public async Task<Articulo?> ObtenerArticuloPorIdAsync(int id) =>
        await Query().FirstOrDefaultAsync(x => x.IdArticulo == id);

    public async Task<Articulo> CrearArticuloAsync(Articulo articulo)
    {
        ArgumentNullException.ThrowIfNull(articulo);
        var entity = new SGPla.Data.NewModel.Entities.Articulo { Numero = articulo.Numero.Trim(), Descripcion = articulo.Descripcion };
        _db.Articulos.Add(entity);
        await _db.SaveChangesAsync();
        articulo.IdArticulo = entity.Id;
        return articulo;
    }

    public async Task<Articulo?> ActualizarArticuloAsync(Articulo articulo)
    {
        var entity = await _db.Articulos.FirstOrDefaultAsync(x => x.Id == articulo.IdArticulo);
        if (entity is null) return null;
        entity.Numero = articulo.Numero.Trim();
        entity.Descripcion = articulo.Descripcion;
        _db.Entry(entity).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return ToLegacy(entity);
    }

    public async Task<bool> EliminarArticuloAsync(int id)
    {
        if (await _db.Avisos.AsNoTracking().AnyAsync(x => x.ArticuloId == id)) return false;
        var entity = await _db.Articulos.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return false;
        _db.Articulos.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Articulo?> ExisteAsync(string numero) =>
        await Query().FirstOrDefaultAsync(x => x.Numero == numero);

    public async Task<IEnumerable<Articulo>> BuscarPorTerminoAsync(string busqueda)
    {
        var texto = busqueda?.Trim() ?? string.Empty;
        return await Query().Where(x => texto == string.Empty || x.Numero.Contains(texto) || (x.Descripcion ?? string.Empty).Contains(texto)).ToListAsync();
    }

    public async Task<Articulo?> ObtenerArticuloPorNumero(int numero) =>
        await Query().FirstOrDefaultAsync(x => x.Numero == numero.ToString());

    private IQueryable<Articulo> Query() => _db.Articulos.AsNoTracking().Select(x => new Articulo
    {
        IdArticulo = x.Id,
        Numero = x.Numero,
        Descripcion = x.Descripcion ?? string.Empty
    });

    private static Articulo ToLegacy(SGPla.Data.NewModel.Entities.Articulo x) => new()
    {
        IdArticulo = x.Id, Numero = x.Numero, Descripcion = x.Descripcion ?? string.Empty
    };
}
