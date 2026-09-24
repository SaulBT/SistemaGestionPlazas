using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Models.DTOs.Docentes;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

public sealed class NormalizedDocenteDirectorioMvcRepository : IDocenteDirectorioMvcRepository
{
    private readonly SgplaDbContext _db;

    public NormalizedDocenteDirectorioMvcRepository(SgplaDbContext db) => _db = db;

    public async Task<PaginaDocenteDirectorioMvc> BuscarAsync(
        DocenteDirectorioMvcFiltro filtro, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        var query = _db.Docentes.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var busqueda = filtro.Busqueda.Trim();
            query = query.Where(x => x.Nombre.Contains(busqueda) || (x.NumPersonal != null && x.NumPersonal.Contains(busqueda)));
        }

        var total = await query.CountAsync(cancellationToken);
        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.TamanoPagina, 1, 100);
        var items = await query.OrderBy(x => x.Nombre).ThenBy(x => x.NumPersonal).ThenBy(x => x.Id)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .Select(x => new DocenteDirectorioMvcFila(x.Id, x.Nombre, x.NumPersonal))
            .ToListAsync(cancellationToken);
        return new PaginaDocenteDirectorioMvc(items, total);
    }
}
