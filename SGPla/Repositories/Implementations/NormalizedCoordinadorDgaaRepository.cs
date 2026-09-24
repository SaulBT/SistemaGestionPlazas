using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Models;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

public sealed class NormalizedCoordinadorDgaaRepository : ICoordinadorDgaaRepository
{
    private const byte RolDgaa = 2;
    private readonly SgplaDbContext _db;
    private readonly TimeProvider _timeProvider;
    public NormalizedCoordinadorDgaaRepository(SgplaDbContext db, TimeProvider? timeProvider = null)
    {
        _db = db;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<int> CrearAsync(CoordinadorDgaa value)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        await ValidarAreaAcademicaVigenteAsync(value.IdAreaAcademica);
        var user = new SGPla.Data.NewModel.Entities.Usuario { Nombre = value.Nombre.Trim(), Correo = value.Correo.Trim().ToLowerInvariant(), RolId = RolDgaa };
        _db.Usuarios.Add(user);
        await _db.SaveChangesAsync();
        _db.UsuariosDgaa.Add(new SGPla.Data.NewModel.Entities.UsuarioDgaa { UsuarioId = user.Id, AreaAcademicaId = value.IdAreaAcademica });
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return user.Id;
    }

    public Task<bool> ExisteCorreoAsync(string correo) =>
        _db.Usuarios.AsNoTracking().AnyAsync(x => x.Correo == correo && x.RolId == RolDgaa && x.FechaEliminacion == null);

    public async Task<CoordinadorDgaa?> ObtenerPorIdAsync(int id) => ToDto(await Consulta(usuarioId: id).FirstOrDefaultAsync());

    public async Task<CoordinadorDgaa?> ObtenerPorCorreoAsync(string correo) => ToDto(await Consulta(correo: correo).FirstOrDefaultAsync());

    public Task<List<CoordinadorDgaa>> ObtenerTodosAsync() => MaterializarAsync(Consulta());

    public Task<List<CoordinadorDgaa>> BuscarConFiltros(int? idAreaAcademica, string? busqueda) =>
        MaterializarAsync(Consulta(idAreaAcademica: idAreaAcademica, busqueda: busqueda));

    public async Task ActualizarAsync(CoordinadorDgaa value)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        await ValidarAreaAcademicaVigenteAsync(value.IdAreaAcademica);
        var user = await _db.Usuarios.AsTracking().FirstOrDefaultAsync(x => x.Id == value.IdCoordinadorDgaa && x.RolId == RolDgaa && x.FechaEliminacion == null)
            ?? throw new InvalidOperationException("No se encontró el coordinador DGAA.");
        user.Nombre = value.Nombre.Trim();
        var link = await _db.UsuariosDgaa.AsTracking().FirstOrDefaultAsync(x => x.UsuarioId == user.Id)
            ?? throw new InvalidOperationException("El coordinador no tiene perfil DGAA.");
        link.AreaAcademicaId = value.IdAreaAcademica;
        _db.Entry(user).State = EntityState.Modified;
        _db.Entry(link).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task EliminarAsync(CoordinadorDgaa value)
    {
        var user = await _db.Usuarios.AsTracking().FirstOrDefaultAsync(x => x.Id == value.IdCoordinadorDgaa && x.RolId == RolDgaa && x.FechaEliminacion == null);
        if (user is null) return;
        user.FechaEliminacion = _timeProvider.GetUtcNow().UtcDateTime;
        _db.Entry(user).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    private async Task ValidarAreaAcademicaVigenteAsync(int areaAcademicaId)
    {
        if (areaAcademicaId < 1 || !await _db.AreaAcademicas.AsNoTracking()
                .AnyAsync(x => x.Id == areaAcademicaId && x.FechaEliminacion == null))
            throw new InvalidOperationException("El Área Académica seleccionada no está vigente.");
    }

    public async Task<SuperUsuario?> ObtenerSuperUsuarioPorCorreoAsync(string correo)
    {
        var user = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Correo == correo && x.RolId == 1 && x.FechaEliminacion == null);
        return user is null ? null : new SuperUsuario { IdSuperUsuario = user.Id, Nombre = user.Nombre, Correo = user.Correo };
    }

    private IQueryable<Row> Consulta(int? usuarioId = null, string? correo = null, int? idAreaAcademica = null, string? busqueda = null)
    {
        var query = from link in _db.UsuariosDgaa.AsNoTracking()
                    join user in _db.Usuarios.AsNoTracking() on link.UsuarioId equals user.Id
                    join area in _db.AreaAcademicas.AsNoTracking() on link.AreaAcademicaId equals area.Id
                    where user.RolId == RolDgaa && user.FechaEliminacion == null && area.FechaEliminacion == null
                    select new { link, user, area };
        if (usuarioId.HasValue) query = query.Where(x => x.user.Id == usuarioId.Value);
        if (!string.IsNullOrWhiteSpace(correo)) query = query.Where(x => x.user.Correo == correo);
        if (idAreaAcademica.HasValue) query = query.Where(x => x.link.AreaAcademicaId == idAreaAcademica.Value);
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim();
            query = query.Where(x => x.user.Nombre.Contains(texto) || x.user.Correo.Contains(texto));
        }
        return query.Select(x => new Row(x.user.Id, x.user.Nombre, x.user.Correo, x.link.AreaAcademicaId, x.area.Nombre));
    }

    private async Task<List<CoordinadorDgaa>> MaterializarAsync(IQueryable<Row> query) =>
        (await query.ToListAsync()).Select(x => ToDto(x)!).ToList();

    private static CoordinadorDgaa? ToDto(Row? x) => x is null ? null : new CoordinadorDgaa
    {
        IdCoordinadorDgaa = x.UserId,
        IdAreaAcademica = x.AreaId,
        Nombre = x.Nombre,
        Correo = x.Correo,
        Cargo = string.Empty,
        IdAreaAcademicaNavigation = new AreaAcademica { IdAreaAcademica = x.AreaId, Nombre = x.AreaNombre, Telefono = string.Empty }
    };

    private sealed record Row(int UserId, string Nombre, string Correo, int AreaId, string AreaNombre)
    {
        public string? Cargo => null;
    }
}
