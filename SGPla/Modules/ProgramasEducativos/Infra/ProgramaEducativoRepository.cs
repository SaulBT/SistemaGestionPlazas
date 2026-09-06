using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Modules.ProgramasEducativos.Application.Models;
using SGPla.Modules.ProgramasEducativos.Application.Ports;
using System.Linq.Expressions;

namespace SGPla.Modules.ProgramasEducativos.Infra;

public sealed class ProgramaEducativoRepository : IProgramaEducativoRepository
{
    private static readonly Expression<Func<ProgramaEducativo, ProgramaEducativoRegistro>> Proyeccion =
        programa => new ProgramaEducativoRegistro(
            programa.IdProgramaEducativo,
            programa.Nombre,
            programa.Campus,
            programa.IdEntidadAcademica,
            programa.IdEntidadAcademicaNavigation.Nombre,
            programa.IdEntidadAcademicaNavigation.IdAreaAcademica,
            programa.IdEntidadAcademicaNavigation.IdAreaAcademicaNavigation.Nombre,
            programa.IdEntidadAcademicaNavigation.Region);

    private readonly GestionDePlazasDbContext _context;

    public ProgramaEducativoRepository(GestionDePlazasDbContext context)
    {
        _context = context;
    }

    public async Task<ProgramasEducativosPagina> ObtenerPorFiltroAsync(
        ProgramaEducativoFiltro filtro,
        CancellationToken cancellationToken)
    {
        var query = _context.ProgramaEducativo
            .AsNoTracking()
            .Where(programa => programa.FechaEliminacion == null);

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var busqueda = filtro.Busqueda.Trim();
            query = query.Where(programa => programa.Nombre.Contains(busqueda));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Region))
        {
            query = query.Where(programa =>
                programa.IdEntidadAcademicaNavigation.Region == filtro.Region);
        }

        if (filtro.IdAreaAcademica.HasValue)
        {
            query = query.Where(programa =>
                programa.IdEntidadAcademicaNavigation.IdAreaAcademica == filtro.IdAreaAcademica.Value);
        }

        if (filtro.IdEntidadAcademica.HasValue)
        {
            query = query.Where(programa =>
                programa.IdEntidadAcademica == filtro.IdEntidadAcademica.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var saltar = (filtro.Pagina - 1) * filtro.Cantidad;

        var items = await query
            .OrderBy(programa => programa.Nombre)
            .Skip(saltar)
            .Take(filtro.Cantidad)
            .Select(Proyeccion)
            .ToListAsync(cancellationToken);

        return new ProgramasEducativosPagina(items, total);
    }

    public async Task<ProgramaEducativoRegistro?> ObtenerPorIdAsync(
        int idProgramaEducativo,
        CancellationToken cancellationToken)
    {
        return await _context.ProgramaEducativo
            .AsNoTracking()
            .Where(programa => programa.IdProgramaEducativo == idProgramaEducativo
                && programa.FechaEliminacion == null)
            .Select(Proyeccion)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> ExisteEntidadAcademicaActivaAsync(
        int idEntidadAcademica,
        CancellationToken cancellationToken)
    {
        return _context.EntidadAcademica
            .AsNoTracking()
            .AnyAsync(entidad => entidad.IdEntidadAcademica == idEntidadAcademica
                && entidad.FechaEliminacion == null,
                cancellationToken);
    }

    public Task<bool> ExistePorClaveAsync(
        string clave,
        int? idProgramaEducativoExcluido,
        CancellationToken cancellationToken)
    {
        var prefijoClave = $"{clave}-";

        return _context.ProgramaEducativo
            .AsNoTracking()
            .AnyAsync(programa =>
                (!idProgramaEducativoExcluido.HasValue
                    || programa.IdProgramaEducativo != idProgramaEducativoExcluido.Value)
                && programa.Nombre.StartsWith(prefijoClave),
                cancellationToken);
    }

    public async Task<ProgramaEducativoRegistro> CrearAsync(
        ProgramaEducativoParaCrear programaEducativo,
        CancellationToken cancellationToken)
    {
        var entidad = new ProgramaEducativo
        {
            Nombre = programaEducativo.Nombre,
            Campus = programaEducativo.Campus,
            IdEntidadAcademica = programaEducativo.IdEntidadAcademica
        };

        _context.ProgramaEducativo.Add(entidad);
        await _context.SaveChangesAsync(cancellationToken);

        return await ObtenerPorIdAsync(entidad.IdProgramaEducativo, cancellationToken)
            ?? throw new InvalidOperationException(
                "No fue posible recuperar el programa educativo creado.");
    }

    public async Task<ProgramaEducativoRegistro?> ActualizarAsync(
        ProgramaEducativoParaActualizar programaEducativo,
        CancellationToken cancellationToken)
    {
        var entidad = await _context.ProgramaEducativo
            .FirstOrDefaultAsync(
                programa => programa.IdProgramaEducativo == programaEducativo.IdProgramaEducativo
                    && programa.FechaEliminacion == null,
                cancellationToken);

        if (entidad is null)
        {
            return null;
        }

        entidad.Nombre = programaEducativo.Nombre;
        entidad.Campus = programaEducativo.Campus;
        entidad.IdEntidadAcademica = programaEducativo.IdEntidadAcademica;
        await _context.SaveChangesAsync(cancellationToken);

        return await ObtenerPorIdAsync(entidad.IdProgramaEducativo, cancellationToken);
    }

    public async Task<bool> EliminarAsync(
        int idProgramaEducativo,
        DateTime fechaEliminacion,
        CancellationToken cancellationToken)
    {
        var entidad = await _context.ProgramaEducativo
            .FirstOrDefaultAsync(
                programa => programa.IdProgramaEducativo == idProgramaEducativo
                    && programa.FechaEliminacion == null,
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
