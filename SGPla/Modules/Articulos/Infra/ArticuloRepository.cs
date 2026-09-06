using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Modules.Articulos.Application.Models;
using SGPla.Modules.Articulos.Application.Ports;

namespace SGPla.Modules.Articulos.Infra;

public sealed class ArticuloRepository : IArticuloRepository
{
    private readonly GestionDePlazasDbContext _context;

    public ArticuloRepository(GestionDePlazasDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ArticuloConsulta>> ObtenerTodosAsync(
        string? busqueda,
        CancellationToken cancellationToken)
    {
        var query = _context.Articulo
            .AsNoTracking()
            .AsQueryable();

        var texto = busqueda?.Trim();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            query = query.Where(articulo =>
                articulo.Numero.Contains(texto)
                || articulo.Descripcion.Contains(texto));
        }

        return await query
            .OrderBy(articulo => articulo.Numero)
            .Select(articulo => new ArticuloConsulta(
                articulo.IdArticulo,
                articulo.Numero,
                articulo.Descripcion))
            .ToListAsync(cancellationToken);
    }

    public async Task<ArticuloConsulta?> ObtenerPorIdAsync(
        int idArticulo,
        CancellationToken cancellationToken)
    {
        return await _context.Articulo
            .AsNoTracking()
            .Where(articulo => articulo.IdArticulo == idArticulo)
            .Select(articulo => new ArticuloConsulta(
                articulo.IdArticulo,
                articulo.Numero,
                articulo.Descripcion))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> ExistePorNumeroAsync(
        string numero,
        int? idArticuloExcluido,
        CancellationToken cancellationToken)
    {
        return _context.Articulo
            .AsNoTracking()
            .AnyAsync(articulo =>
                articulo.Numero == numero
                && (!idArticuloExcluido.HasValue
                    || articulo.IdArticulo != idArticuloExcluido.Value),
                cancellationToken);
    }

    public async Task<ArticuloConsulta> CrearAsync(
        ArticuloParaCrear articulo,
        CancellationToken cancellationToken)
    {
        var entidad = new Articulo
        {
            Numero = articulo.Numero,
            Descripcion = articulo.Descripcion
        };

        _context.Articulo.Add(entidad);
        await _context.SaveChangesAsync(cancellationToken);

        return new ArticuloConsulta(
            entidad.IdArticulo,
            entidad.Numero,
            entidad.Descripcion);
    }

    public async Task<ArticuloConsulta?> ActualizarAsync(
        ArticuloParaActualizar articulo,
        CancellationToken cancellationToken)
    {
        var entidad = await _context.Articulo
            .FirstOrDefaultAsync(
                elemento => elemento.IdArticulo == articulo.IdArticulo,
                cancellationToken);

        if (entidad is null)
        {
            return null;
        }

        entidad.Numero = articulo.Numero;
        entidad.Descripcion = articulo.Descripcion;
        await _context.SaveChangesAsync(cancellationToken);

        return new ArticuloConsulta(
            entidad.IdArticulo,
            entidad.Numero,
            entidad.Descripcion);
    }

    public async Task<bool> EliminarAsync(
        int idArticulo,
        CancellationToken cancellationToken)
    {
        var entidad = await _context.Articulo
            .FirstOrDefaultAsync(
                articulo => articulo.IdArticulo == idArticulo,
                cancellationToken);

        if (entidad is null)
        {
            return false;
        }

        _context.Articulo.Remove(entidad);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
