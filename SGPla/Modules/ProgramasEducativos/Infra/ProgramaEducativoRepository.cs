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
            .Where(programa => programa.FechaEliminacion == null
                && programa.IdEntidadAcademicaNavigation.FechaEliminacion == null);

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

        var idsProgramas = items.Select(item => item.IdProgramaEducativo).ToArray();
        var planes = await _context.PlanEstudios
            .AsNoTracking()
            .Where(plan => idsProgramas.Contains(plan.IdProgramaEducativo))
            .OrderBy(plan => plan.Nombre)
            .ThenBy(plan => plan.IdPlanEstudios)
            .Select(plan => new
            {
                plan.IdProgramaEducativo,
                Registro = new PlanEstudioResumenRegistro(
                    plan.IdPlanEstudios,
                    plan.Nombre)
            })
            .ToListAsync(cancellationToken);

        var planesPorPrograma = planes
            .GroupBy(item => item.IdProgramaEducativo)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => (IReadOnlyList<PlanEstudioResumenRegistro>)grupo
                    .Select(item => item.Registro)
                    .ToList());

        items = items
            .Select(item => item with
            {
                PlanesEstudio = planesPorPrograma.TryGetValue(
                    item.IdProgramaEducativo,
                    out var planesDelPrograma)
                    ? planesDelPrograma
                    : Array.Empty<PlanEstudioResumenRegistro>()
            })
            .ToList();

        return new ProgramasEducativosPagina(items, total);
    }

    public async Task<ProgramaEducativoRegistro?> ObtenerPorIdAsync(
        int idProgramaEducativo,
        CancellationToken cancellationToken)
    {
        return await _context.ProgramaEducativo
            .AsNoTracking()
            .Where(programa => programa.IdProgramaEducativo == idProgramaEducativo
                && programa.FechaEliminacion == null
                && programa.IdEntidadAcademicaNavigation.FechaEliminacion == null)
            .Select(Proyeccion)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PlanEstudioRegistro>> ObtenerPlanesEstudioAsync(
        int idProgramaEducativo,
        CancellationToken cancellationToken)
    {
        return await _context.PlanEstudios
            .AsNoTracking()
            .Where(plan => plan.IdProgramaEducativo == idProgramaEducativo)
            .Where(plan => plan.IdProgramaEducativoNavigation.FechaEliminacion == null
                && plan.IdProgramaEducativoNavigation.IdEntidadAcademicaNavigation
                    .FechaEliminacion == null)
            .OrderBy(plan => plan.Nombre)
            .ThenBy(plan => plan.Modalidad)
            .Select(plan => new PlanEstudioRegistro(
                plan.IdPlanEstudios,
                plan.Nombre,
                plan.Modalidad,
                plan.IdArchivoPlan,
                plan.IdArchivoPlanNavigation == null ? null : plan.IdArchivoPlanNavigation.Nombre,
                plan.IdArchivoPlanNavigation == null ? null : plan.IdArchivoPlanNavigation.Ruta,
                plan.IdArchivoPlanNavigation == null ? null : plan.IdArchivoPlanNavigation.Tipo,
                plan.IdArchivoPlanNavigation == null ? null : plan.IdArchivoPlanNavigation.Tamanio,
                plan.ExperienciaEducativa.Count))
            .ToListAsync(cancellationToken);
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

    public async Task<ProgramaEducativoRegistro> CrearConPlanesEstudioAsync(
        ProgramaEducativoParaCrear programaEducativo,
        IReadOnlyList<PlanEstudioParaPersistir> planesEstudio,
        CancellationToken cancellationToken)
    {
        var estrategia = _context.Database.CreateExecutionStrategy();

        return await estrategia.ExecuteAsync(async () =>
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync(
                cancellationToken);

            var programa = new ProgramaEducativo
            {
                Nombre = programaEducativo.Nombre,
                Campus = programaEducativo.Campus,
                IdEntidadAcademica = programaEducativo.IdEntidadAcademica
            };

            foreach (var plan in planesEstudio)
            {
                programa.PlanEstudios.Add(CrearEntidadPlanEstudios(plan));
            }

            _context.ProgramaEducativo.Add(programa);
            await _context.SaveChangesAsync(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);

            return await ObtenerPorIdAsync(programa.IdProgramaEducativo, cancellationToken)
                ?? throw new InvalidOperationException(
                    "No fue posible recuperar el programa educativo creado.");
        });
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

    public async Task<ProgramaActualizacionConPlanesResultado?> ActualizarConPlanesEstudioAsync(
        ProgramaEducativoParaActualizar programaEducativo,
        IReadOnlyList<PlanEstudioParaPersistir> planesEstudio,
        CancellationToken cancellationToken)
    {
        var estrategia = _context.Database.CreateExecutionStrategy();

        return await estrategia.ExecuteAsync(async () =>
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync(
                cancellationToken);

            var programa = await _context.ProgramaEducativo
                .AsTracking()
                .AsSplitQuery()
                .Include(item => item.PlanEstudios)
                    .ThenInclude(plan => plan.IdArchivoPlanNavigation)
                .Include(item => item.PlanEstudios)
                    .ThenInclude(plan => plan.ExperienciaEducativa)
                .FirstOrDefaultAsync(
                    item => item.IdProgramaEducativo == programaEducativo.IdProgramaEducativo
                        && item.FechaEliminacion == null,
                    cancellationToken);

            if (programa is null)
            {
                await transaccion.RollbackAsync(cancellationToken);
                return null;
            }

            programa.Nombre = programaEducativo.Nombre;
            programa.Campus = programaEducativo.Campus;
            programa.IdEntidadAcademica = programaEducativo.IdEntidadAcademica;

            var planesActualesPorId = programa.PlanEstudios.ToDictionary(
                plan => plan.IdPlanEstudios);
            var idsPlanesRecibidos = planesEstudio
                .Where(plan => plan.IdPlanEstudios.HasValue)
                .Select(plan => plan.IdPlanEstudios!.Value)
                .ToHashSet();
            var rutasArchivosAnteriores = new List<string>();

            foreach (var plan in programa.PlanEstudios.ToList())
            {
                if (idsPlanesRecibidos.Contains(plan.IdPlanEstudios))
                {
                    continue;
                }

                AgregarRutaArchivo(plan, rutasArchivosAnteriores);
                _context.ExperienciaEducativa.RemoveRange(plan.ExperienciaEducativa);
                _context.PlanEstudios.Remove(plan);
                if (plan.IdArchivoPlanNavigation is not null)
                {
                    _context.Archivo.Remove(plan.IdArchivoPlanNavigation);
                }
            }

            foreach (var planRecibido in planesEstudio)
            {
                if (!planRecibido.IdPlanEstudios.HasValue)
                {
                    programa.PlanEstudios.Add(CrearEntidadPlanEstudios(planRecibido));
                    continue;
                }

                var planActual = planesActualesPorId[planRecibido.IdPlanEstudios.Value];
                planActual.Nombre = planRecibido.Nombre;
                planActual.Modalidad = planRecibido.Modalidad;

                if (planRecibido.ArchivoNuevo is null)
                {
                    continue;
                }

                AgregarRutaArchivo(planActual, rutasArchivosAnteriores);
                if (planActual.IdArchivoPlanNavigation is null)
                {
                    planActual.IdArchivoPlanNavigation = CrearEntidadArchivo(planRecibido.ArchivoNuevo);
                }
                else
                {
                    ActualizarArchivo(planActual.IdArchivoPlanNavigation, planRecibido.ArchivoNuevo);
                }
                _context.ExperienciaEducativa.RemoveRange(planActual.ExperienciaEducativa);
                planActual.ExperienciaEducativa.Clear();

                foreach (var experiencia in planRecibido.ExperienciasEducativas)
                {
                    planActual.ExperienciaEducativa.Add(CrearEntidadExperiencia(experiencia));
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);

            var registro = await ObtenerPorIdAsync(programa.IdProgramaEducativo, cancellationToken)
                ?? throw new InvalidOperationException(
                    "No fue posible recuperar el programa educativo actualizado.");

            return new ProgramaActualizacionConPlanesResultado(
                registro,
                rutasArchivosAnteriores);
        });
    }

    public async Task<bool> PlanesEstudioTienenDependenciasAsync(
        IReadOnlyCollection<int> idsPlanesEstudio,
        CancellationToken cancellationToken)
    {
        if (idsPlanesEstudio.Count == 0)
        {
            return false;
        }

        if (await _context.SolicitudApertura
                .AsNoTracking()
                .AnyAsync(
                    solicitud => idsPlanesEstudio.Contains(solicitud.IdPlanEstudios),
                    cancellationToken))
        {
            return true;
        }

        var idsExperiencias = _context.ExperienciaEducativa
            .AsNoTracking()
            .Where(experiencia => idsPlanesEstudio.Contains(experiencia.IdPlanEstudios))
            .Select(experiencia => experiencia.IdExperienciaEducativa);

        return await _context.Oferta
            .AsNoTracking()
            .AnyAsync(
                oferta => idsExperiencias.Contains(oferta.IdExperienciaEducativa),
                cancellationToken);
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

    private static PlanEstudios CrearEntidadPlanEstudios(
        PlanEstudioParaPersistir plan)
    {
        var entidadPlan = new PlanEstudios
        {
            Nombre = plan.Nombre,
            Modalidad = plan.Modalidad,
            IdArchivoPlanNavigation = plan.ArchivoNuevo is null
                ? new Archivo()
                : CrearEntidadArchivo(plan.ArchivoNuevo),
        };

        foreach (var experiencia in plan.ExperienciasEducativas)
        {
            entidadPlan.ExperienciaEducativa.Add(CrearEntidadExperiencia(experiencia));
        }

        return entidadPlan;
    }

    private static ExperienciaEducativa CrearEntidadExperiencia(
        ExperienciaEducativaParaCrear experiencia)
    {
        return new ExperienciaEducativa
        {
            Codigo = experiencia.Codigo,
            Nombre = experiencia.Nombre,
            PerfilDocente = experiencia.PerfilDocente,
            Horas = experiencia.Horas,
            Creditos = experiencia.Creditos
        };
    }

    private static Archivo CrearEntidadArchivo(ArchivoPlanGuardado archivo)
    {
        return new Archivo
        {
            Nombre = archivo.NombreOriginal,
            Ruta = archivo.Ruta,
            Tipo = archivo.Tipo,
            Tamanio = archivo.Tamanio
        };
    }

    private static void ActualizarArchivo(Archivo archivo, ArchivoPlanGuardado actualizado)
    {
        archivo.Nombre = actualizado.NombreOriginal;
        archivo.Ruta = actualizado.Ruta;
        archivo.Tipo = actualizado.Tipo;
        archivo.Tamanio = actualizado.Tamanio;
    }

    private static void AgregarRutaArchivo(
        PlanEstudios plan,
        ICollection<string> rutas)
    {
        if (!string.IsNullOrWhiteSpace(plan.IdArchivoPlanNavigation?.Ruta))
        {
            rutas.Add(plan.IdArchivoPlanNavigation.Ruta!);
        }
    }
}
