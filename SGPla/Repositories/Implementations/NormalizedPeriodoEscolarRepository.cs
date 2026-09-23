using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Models;
using SGPla.Models.DTOs.PeriodoEscolar;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

/// <summary>Adaptador del CRUD MVC de periodos al esquema academico normalizado.</summary>
public sealed class NormalizedPeriodoEscolarRepository : IPeriodoEscolarRepository
{
    private readonly SgplaDbContext _db;

    public NormalizedPeriodoEscolarRepository(SgplaDbContext db) => _db = db;

    public async Task<List<Periodo>> ObtenerTodosAsync() =>
        await Query().OrderByDescending(x => x.Codigo).ToListAsync();

    public async Task<Periodo?> ObtenerPorIdAsync(int idPeriodoEscolar) =>
        await Query().FirstOrDefaultAsync(x => x.IdPeriodo == idPeriodoEscolar);

    public async Task<List<Periodo>> ObtenerPorFiltroAsync(BuscarPeriodoEscolarDTO filtro)
    {
        var query = Query();
        if (int.TryParse(filtro.Anio, out var anio) && anio > 0)
            query = query.Where(x => x.Codigo.StartsWith(anio.ToString()));
        if (!string.IsNullOrWhiteSpace(filtro.PeriodoCodigo))
            query = query.Where(x => x.Codigo.EndsWith(filtro.PeriodoCodigo));
        var pagina = Math.Max(filtro.Pagina, 1);
        var cantidad = Math.Max(filtro.Cantidad, 1);
        return await query.OrderByDescending(x => x.Codigo).Skip((pagina - 1) * cantidad).Take(cantidad).ToListAsync();
    }

    public async Task<int> ContarPorFiltroAsync(BuscarPeriodoEscolarDTO filtro)
    {
        var query = Query();
        if (int.TryParse(filtro.Anio, out var anio) && anio > 0)
            query = query.Where(x => x.Codigo.StartsWith(anio.ToString()));
        if (!string.IsNullOrWhiteSpace(filtro.PeriodoCodigo))
            query = query.Where(x => x.Codigo.EndsWith(filtro.PeriodoCodigo));
        return await query.CountAsync();
    }

    public async Task<Periodo?> ExisteAsync(Periodo periodoEscolar) =>
        await Query().FirstOrDefaultAsync(x => x.Codigo == periodoEscolar.Codigo);

    public async Task<Periodo> CrearAsync(Periodo periodoEscolar)
    {
        var fechas = Fechas(periodoEscolar.Codigo);
        var entity = new SGPla.Data.NewModel.Entities.PeriodoEscolar
        {
            Clave = periodoEscolar.Codigo,
            FechaInicio = fechas.Inicio,
            FechaFin = fechas.Fin
        };
        _db.PeriodosEscolares.Add(entity);
        await _db.SaveChangesAsync();
        return ToLegacy(entity);
    }

    public async Task<Periodo?> ActualizarAsync(Periodo periodoEscolar)
    {
        var entity = await _db.PeriodosEscolares.FirstOrDefaultAsync(x => x.Id == periodoEscolar.IdPeriodo && x.FechaEliminacion == null);
        if (entity is null) return null;
        var fechas = Fechas(periodoEscolar.Codigo);
        entity.Clave = periodoEscolar.Codigo;
        entity.FechaInicio = fechas.Inicio;
        entity.FechaFin = fechas.Fin;
        _db.Entry(entity).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return ToLegacy(entity);
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var entity = await _db.PeriodosEscolares.FirstOrDefaultAsync(x => x.Id == id && x.FechaEliminacion == null);
        if (entity is null) return false;
        entity.FechaEliminacion = DateTime.UtcNow;
        _db.Entry(entity).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TieneRelacionesAsync(int idPeriodo)
    {
        if (await _db.ProgramacionAcademicas.AsNoTracking().AnyAsync(x => x.PeriodoEscolarId == idPeriodo && x.FechaEliminacion == null))
            return true;
        return await _db.Avisos.AsNoTracking().AnyAsync(x => x.PeriodoEscolarId == idPeriodo);
    }

    private IQueryable<Periodo> Query() => _db.PeriodosEscolares.AsNoTracking()
        .Where(x => x.FechaEliminacion == null)
        .Select(x => new Periodo { IdPeriodo = x.Id, Codigo = x.Clave, FechaInicio = x.FechaInicio, FechaFin = x.FechaFin });

    private static Periodo ToLegacy(SGPla.Data.NewModel.Entities.PeriodoEscolar x) => new()
    {
        IdPeriodo = x.Id, Codigo = x.Clave, FechaInicio = x.FechaInicio, FechaFin = x.FechaFin
    };

    private static (DateOnly Inicio, DateOnly Fin) Fechas(string codigo)
    {
        if (codigo is null || codigo.Length != 6 || !int.TryParse(codigo[..4], out var anio))
            throw new ArgumentException("El código del periodo debe tener formato AAAAPP.", nameof(codigo));
        return codigo[4..] switch
        {
            "51" => (new DateOnly(anio, 2, 1), new DateOnly(anio, 7, 31)),
            "01" => (new DateOnly(anio - 1, 8, 1), new DateOnly(anio, 1, 31)),
            _ => throw new ArgumentException("El periodo debe terminar en 01 o 51.", nameof(codigo))
        };
    }
}
