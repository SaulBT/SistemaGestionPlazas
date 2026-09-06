using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Modules.EntidadesAcademicas.Application.Models;
using SGPla.Modules.EntidadesAcademicas.Application.Ports;
using System.Linq.Expressions;

namespace SGPla.Modules.EntidadesAcademicas.Infra;

public sealed class EntidadAcademicaRepository : IEntidadAcademicaRepository
{
    private static readonly Expression<Func<EntidadAcademica, EntidadAcademicaRegistro>> Proyeccion =
        entidad => new EntidadAcademicaRegistro(
            entidad.IdEntidadAcademica,
            entidad.Clave,
            entidad.Nombre,
            entidad.CalleNumero,
            entidad.Colonia,
            entidad.Cp,
            entidad.Municipio,
            entidad.Telefono,
            entidad.Extension,
            entidad.IdAreaAcademica,
            entidad.IdAreaAcademicaNavigation.Nombre,
            entidad.Region);

    private readonly GestionDePlazasDbContext _context;

    public EntidadAcademicaRepository(GestionDePlazasDbContext context)
    {
        _context = context;
    }

    public async Task<EntidadesAcademicasPagina> ObtenerPorFiltroAsync(
        EntidadAcademicaFiltro filtro,
        CancellationToken cancellationToken)
    {
        var query = _context.EntidadAcademica
            .AsNoTracking()
            .Where(entidad => entidad.FechaEliminacion == null);

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var busqueda = filtro.Busqueda.Trim();
            query = query.Where(entidad =>
                (entidad.Clave != null && entidad.Clave.Contains(busqueda))
                || entidad.Nombre.Contains(busqueda));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Region))
        {
            query = query.Where(entidad => entidad.Region == filtro.Region);
        }

        if (filtro.IdAreaAcademica.HasValue)
        {
            query = query.Where(entidad =>
                entidad.IdAreaAcademica == filtro.IdAreaAcademica.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var saltar = (filtro.Pagina - 1) * filtro.Cantidad;

        var items = await query
            .OrderBy(entidad => entidad.Nombre)
            .ThenBy(entidad => entidad.Clave)
            .Skip(saltar)
            .Take(filtro.Cantidad)
            .Select(Proyeccion)
            .ToListAsync(cancellationToken);

        return new EntidadesAcademicasPagina(items, total);
    }

    public async Task<EntidadAcademicaRegistro?> ObtenerPorIdAsync(
        int idEntidadAcademica,
        CancellationToken cancellationToken)
    {
        return await _context.EntidadAcademica
            .AsNoTracking()
            .Where(entidad => entidad.IdEntidadAcademica == idEntidadAcademica
                && entidad.FechaEliminacion == null)
            .Select(Proyeccion)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> ExisteAreaAcademicaActivaAsync(
        int idAreaAcademica,
        CancellationToken cancellationToken)
    {
        return _context.AreaAcademica
            .AsNoTracking()
            .AnyAsync(area => area.IdAreaAcademica == idAreaAcademica
                && area.FechaEliminacion == null,
                cancellationToken);
    }

    public Task<bool> ExistePorClaveAsync(
        string clave,
        int? idEntidadAcademicaExcluida,
        CancellationToken cancellationToken)
    {
        var prefijoClave = $"{clave}-";

        return _context.EntidadAcademica
            .AsNoTracking()
            .AnyAsync(entidad =>
                (!idEntidadAcademicaExcluida.HasValue
                    || entidad.IdEntidadAcademica != idEntidadAcademicaExcluida.Value)
                && (entidad.Clave == clave
                    || (entidad.Clave == null
                        && entidad.Nombre.StartsWith(prefijoClave))),
                cancellationToken);
    }

    public async Task<EntidadAcademicaRegistro> CrearAsync(
        EntidadAcademicaParaCrear entidadAcademica,
        CancellationToken cancellationToken)
    {
        var entidad = new EntidadAcademica
        {
            Clave = entidadAcademica.Clave,
            Nombre = entidadAcademica.Nombre,
            CalleNumero = entidadAcademica.CalleNumero,
            Colonia = entidadAcademica.Colonia,
            Cp = entidadAcademica.Cp,
            Municipio = entidadAcademica.Municipio,
            Telefono = entidadAcademica.Telefono,
            Extension = entidadAcademica.Extension,
            IdAreaAcademica = entidadAcademica.IdAreaAcademica,
            Region = entidadAcademica.Region
        };

        _context.EntidadAcademica.Add(entidad);
        await _context.SaveChangesAsync(cancellationToken);

        return await ObtenerPorIdAsync(entidad.IdEntidadAcademica, cancellationToken)
            ?? throw new InvalidOperationException(
                "No fue posible recuperar la entidad académica creada.");
    }

    public async Task<EntidadAcademicaRegistro?> ActualizarAsync(
        EntidadAcademicaParaActualizar entidadAcademica,
        CancellationToken cancellationToken)
    {
        var entidad = await _context.EntidadAcademica
            .FirstOrDefaultAsync(
                elemento => elemento.IdEntidadAcademica == entidadAcademica.IdEntidadAcademica
                    && elemento.FechaEliminacion == null,
                cancellationToken);

        if (entidad is null)
        {
            return null;
        }

        entidad.Clave = entidadAcademica.Clave;
        entidad.Nombre = entidadAcademica.Nombre;
        entidad.CalleNumero = entidadAcademica.CalleNumero;
        entidad.Colonia = entidadAcademica.Colonia;
        entidad.Cp = entidadAcademica.Cp;
        entidad.Municipio = entidadAcademica.Municipio;
        entidad.Telefono = entidadAcademica.Telefono;
        entidad.Extension = entidadAcademica.Extension;
        entidad.IdAreaAcademica = entidadAcademica.IdAreaAcademica;
        entidad.Region = entidadAcademica.Region;
        await _context.SaveChangesAsync(cancellationToken);

        return await ObtenerPorIdAsync(entidad.IdEntidadAcademica, cancellationToken);
    }

    public async Task<bool> EliminarAsync(
        int idEntidadAcademica,
        DateTime fechaEliminacion,
        CancellationToken cancellationToken)
    {
        var entidad = await _context.EntidadAcademica
            .FirstOrDefaultAsync(
                elemento => elemento.IdEntidadAcademica == idEntidadAcademica
                    && elemento.FechaEliminacion == null,
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
