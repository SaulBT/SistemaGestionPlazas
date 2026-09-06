using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Modules.DireccionesAreaAcademica.Application.Models;
using SGPla.Modules.DireccionesAreaAcademica.Application.Ports;

namespace SGPla.Modules.DireccionesAreaAcademica.Infra;

public sealed class AreaAcademicaRepository : IAreaAcademicaRepository
{
    private readonly GestionDePlazasDbContext _context;

    public AreaAcademicaRepository(GestionDePlazasDbContext context)
    {
        _context = context;
    }

    public async Task<AreasAcademicasPagina> ObtenerPorFiltroAsync(
        AreaAcademicaFiltro filtro,
        CancellationToken cancellationToken)
    {
        var query = _context.AreaAcademica
            .AsNoTracking()
            .Where(area => area.FechaEliminacion == null);

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            query = query.Where(area => area.Nombre.Contains(filtro.Busqueda));
        }

        var total = await query.CountAsync(cancellationToken);
        var saltar = (filtro.Pagina - 1) * filtro.Cantidad;

        var items = await query
            .OrderBy(area => area.Nombre)
            .Skip(saltar)
            .Take(filtro.Cantidad)
            .Select(area => new AreaAcademicaRegistro(
                area.IdAreaAcademica,
                area.Nombre,
                area.Telefono,
                area.Extension))
            .ToListAsync(cancellationToken);

        return new AreasAcademicasPagina(items, total);
    }

    public async Task<AreaAcademicaRegistro?> ObtenerPorIdAsync(
        int idAreaAcademica,
        CancellationToken cancellationToken)
    {
        return await _context.AreaAcademica
            .AsNoTracking()
            .Where(area => area.IdAreaAcademica == idAreaAcademica
                && area.FechaEliminacion == null)
            .Select(area => new AreaAcademicaRegistro(
                area.IdAreaAcademica,
                area.Nombre,
                area.Telefono,
                area.Extension))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AreaAcademicaRegistro> CrearAsync(
        AreaAcademicaParaCrear areaAcademica,
        CancellationToken cancellationToken)
    {
        var entidad = new AreaAcademica
        {
            Nombre = areaAcademica.Nombre,
            Telefono = areaAcademica.Telefono,
            Extension = areaAcademica.Extension
        };

        _context.AreaAcademica.Add(entidad);
        await _context.SaveChangesAsync(cancellationToken);

        return new AreaAcademicaRegistro(
            entidad.IdAreaAcademica,
            entidad.Nombre,
            entidad.Telefono,
            entidad.Extension);
    }

    public async Task<AreaAcademicaRegistro?> ActualizarAsync(
        AreaAcademicaParaActualizar areaAcademica,
        CancellationToken cancellationToken)
    {
        var entidad = await _context.AreaAcademica
            .FirstOrDefaultAsync(
                area => area.IdAreaAcademica == areaAcademica.IdAreaAcademica
                    && area.FechaEliminacion == null,
                cancellationToken);

        if (entidad is null)
        {
            return null;
        }

        entidad.Nombre = areaAcademica.Nombre;
        entidad.Telefono = areaAcademica.Telefono;
        entidad.Extension = areaAcademica.Extension;
        await _context.SaveChangesAsync(cancellationToken);

        return new AreaAcademicaRegistro(
            entidad.IdAreaAcademica,
            entidad.Nombre,
            entidad.Telefono,
            entidad.Extension);
    }

    public async Task<bool> EliminarAsync(
        int idAreaAcademica,
        DateTime fechaEliminacion,
        CancellationToken cancellationToken)
    {
        var entidad = await _context.AreaAcademica
            .FirstOrDefaultAsync(
                area => area.IdAreaAcademica == idAreaAcademica
                    && area.FechaEliminacion == null,
                cancellationToken);

        if (entidad is null)
        {
            return false;
        }

        entidad.FechaEliminacion = fechaEliminacion;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
