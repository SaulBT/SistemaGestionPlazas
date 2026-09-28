using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.ProgramacionAcademica;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

public sealed class NormalizedProgramacionAcademicaMvcRepository : IProgramacionAcademicaMvcRepository
{
    private readonly SgplaDbContext _db;

    public NormalizedProgramacionAcademicaMvcRepository(SgplaDbContext db) => _db = db;

    public async Task<IReadOnlyList<SGPla.Models.ViewModels.Catalogos.CatalogoOpcion>> ObtenerPeriodosAsync(CancellationToken cancellationToken = default) =>
        await _db.PeriodosEscolares.AsNoTracking().Where(x => x.FechaEliminacion == null)
            .OrderByDescending(x => x.Clave)
            .Select(x => new SGPla.Models.ViewModels.Catalogos.CatalogoOpcion(x.Id, x.Clave, x.Clave))
            .ToListAsync(cancellationToken);

    public async Task<PaginaProgramacionAcademicaMvc> BuscarAsync(
        ProgramacionAcademicaMvcFiltro filtro, int? entidadAcademicaAutorizadaId, CancellationToken cancellationToken = default)
    {
        var query = from programacion in _db.ProgramacionAcademicas.AsNoTracking()
                    join periodo in _db.PeriodosEscolares.AsNoTracking() on programacion.PeriodoEscolarId equals periodo.Id
                    join experiencia in _db.ExperienciasEducativas.AsNoTracking() on programacion.ExperienciaEducativaId equals experiencia.Id
                    join plan in _db.PlanesEstudios.AsNoTracking() on experiencia.PlanEstudiosId equals plan.Id
                    join programa in _db.ProgramasEducativos.AsNoTracking() on plan.ProgramaEducativoId equals programa.Id
                    join entidad in _db.EntidadAcademicas.AsNoTracking() on programa.EntidadAcademicaId equals entidad.Id
                    join campus in _db.Campuses.AsNoTracking() on entidad.CampusId equals campus.Id
                    join region in _db.Regiones.AsNoTracking() on campus.RegionId equals region.Id
                    where programacion.FechaEliminacion == null && periodo.FechaEliminacion == null
                          && experiencia.FechaEliminacion == null && plan.FechaEliminacion == null
                          && programa.FechaEliminacion == null && entidad.FechaEliminacion == null
                          && campus.FechaEliminacion == null && region.FechaEliminacion == null
                    select new { programacion, periodo, experiencia, programa, entidad, region };

        if (entidadAcademicaAutorizadaId.HasValue)
            query = query.Where(x => x.entidad.Id == entidadAcademicaAutorizadaId.Value);
        if (filtro.RegionId.HasValue) query = query.Where(x => x.region.Id == filtro.RegionId.Value);
        if (filtro.EntidadAcademicaId.HasValue) query = query.Where(x => x.entidad.Id == filtro.EntidadAcademicaId.Value);
        if (filtro.ProgramaEducativoId.HasValue) query = query.Where(x => x.programa.Id == filtro.ProgramaEducativoId.Value);
        if (filtro.PeriodoEscolarId.HasValue) query = query.Where(x => x.periodo.Id == filtro.PeriodoEscolarId.Value);
        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var texto = filtro.Busqueda.Trim();
            query = query.Where(x => x.programacion.Nrc.Contains(texto) || x.experiencia.Nombre.Contains(texto)
                || x.experiencia.MateriaEe.Contains(texto) || x.experiencia.CursoEe.Contains(texto));
        }

        var total = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, filtro.Pagina);
        var size = Math.Clamp(filtro.TamanoPagina, 1, 100);
        var items = await query.OrderByDescending(x => x.periodo.Clave).ThenBy(x => x.programa.Nombre)
            .ThenBy(x => x.programacion.Nrc).Skip((page - 1) * size).Take(size)
            .Select(x => new ProgramacionAcademicaMvcFila(
                x.programacion.Id, x.programacion.Nrc, x.periodo.Id, x.periodo.Clave,
                x.entidad.Id, x.entidad.Nombre, x.programa.Id, x.programa.Nombre,
                x.experiencia.Id, x.experiencia.Nombre, x.experiencia.MateriaEe, x.experiencia.CursoEe,
                x.programacion.FechaEliminacion))
            .ToListAsync(cancellationToken);
        return new PaginaProgramacionAcademicaMvc(items, total);
    }

    public async Task<ResultadoImportacionProgramacionMvc> ImportarAsync(
        int periodoEscolarId, int entidadAcademicaId, IReadOnlyList<ProgramacionAcademicaImportacionFila> filas,
        CancellationToken cancellationToken = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        var periodo = await _db.PeriodosEscolares.AsNoTracking()
            .AnyAsync(x => x.Id == periodoEscolarId && x.FechaEliminacion == null, cancellationToken);
        if (!periodo) throw new ArgumentException("Selecciona un periodo escolar vigente.");
        var entidadValida = await (from entidad in _db.EntidadAcademicas.AsNoTracking()
                                   join campus in _db.Campuses.AsNoTracking() on entidad.CampusId equals campus.Id
                                   join region in _db.Regiones.AsNoTracking() on campus.RegionId equals region.Id
                                   join area in _db.AreaAcademicas.AsNoTracking() on entidad.AreaAcademicaId equals area.Id
                                   where entidad.Id == entidadAcademicaId && entidad.FechaEliminacion == null
                                         && campus.FechaEliminacion == null && region.FechaEliminacion == null
                                         && area.FechaEliminacion == null
                                   select entidad.Id).AnyAsync(cancellationToken);
        if (!entidadValida) throw new ArgumentException("Selecciona una Entidad Académica vigente.");

        var nombresPrograma = filas.Select(x => x.Programa).Distinct().ToArray();
        var nombresEe = filas.Select(x => x.ExperienciaEducativa).Distinct().ToArray();
        var candidatos = await (from programa in _db.ProgramasEducativos.AsNoTracking()
                                join plan in _db.PlanesEstudios.AsNoTracking() on programa.Id equals plan.ProgramaEducativoId
                                join ee in _db.ExperienciasEducativas.AsNoTracking() on plan.Id equals ee.PlanEstudiosId
                                where programa.EntidadAcademicaId == entidadAcademicaId
                                      && programa.FechaEliminacion == null && plan.FechaEliminacion == null
                                      && ee.FechaEliminacion == null && nombresPrograma.Contains(programa.Nombre)
                                      && nombresEe.Contains(ee.Nombre)
                                select new { Programa = programa.Nombre, programa.Id, EeNombre = ee.Nombre, EeId = ee.Id })
            .ToListAsync(cancellationToken);

        var referencia = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var fila in filas)
        {
            var coincidencias = candidatos.Where(x =>
                NormalizarTexto(x.Programa) == NormalizarTexto(fila.Programa) &&
                NormalizarTexto(x.EeNombre) == NormalizarTexto(fila.ExperienciaEducativa))
                .Select(x => x.EeId).Distinct().ToArray();
            if (coincidencias.Length != 1)
                throw new ArgumentException(coincidencias.Length == 0
                    ? $"La fila NRC {fila.Nrc} no corresponde a una EE vigente del programa seleccionado."
                    : $"La fila NRC {fila.Nrc} es ambigua porque el programa tiene varias EE con el mismo nombre.");
            referencia[fila.Nrc] = coincidencias[0];
        }

        var nrcs = filas.Select(x => x.Nrc).Distinct().ToArray();
        var existentes = await _db.ProgramacionAcademicas.AsNoTracking()
            .Where(x => x.PeriodoEscolarId == periodoEscolarId && nrcs.Contains(x.Nrc))
            .Select(x => new { x.Nrc, x.ExperienciaEducativaId, x.FechaEliminacion }).ToListAsync(cancellationToken);
        var porNrc = existentes.ToDictionary(x => x.Nrc, StringComparer.OrdinalIgnoreCase);
        var nuevas = new List<ProgramacionAcademica>();
        var yaExistentes = 0;
        foreach (var fila in filas)
        {
            if (porNrc.TryGetValue(fila.Nrc, out var existente))
            {
                if (existente.FechaEliminacion.HasValue || existente.ExperienciaEducativaId != referencia[fila.Nrc])
                    throw new InvalidOperationException($"El NRC {fila.Nrc} ya está asociado a otra EE o fue dado de baja en este periodo.");
                yaExistentes++;
                continue;
            }
            nuevas.Add(new ProgramacionAcademica
            {
                Nrc = fila.Nrc,
                PeriodoEscolarId = periodoEscolarId,
                ExperienciaEducativaId = referencia[fila.Nrc]
            });
        }
        foreach (var nueva in nuevas) _db.Entry(nueva).State = EntityState.Added;
        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return new ResultadoImportacionProgramacionMvc(nuevas.Count, yaExistentes, filas.Count);
    }

    private static string NormalizarTexto(string valor) =>
        new string(valor.Trim().Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray())
        .Normalize(NormalizationForm.FormC).ToUpperInvariant();
}
