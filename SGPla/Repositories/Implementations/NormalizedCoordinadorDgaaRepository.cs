using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Models;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

public sealed class NormalizedCoordinadorDgaaRepository : ICoordinadorDgaaRepository
{
    private const byte RolDgaa = 2;
    private readonly SgplaDbContext _db;
    public NormalizedCoordinadorDgaaRepository(SgplaDbContext db) => _db = db;

    public async Task<int> CrearAsync(CoordinadorDgaa value)
    {
        var user = new SGPla.Data.NewModel.Entities.Usuario { Nombre = value.Nombre.Trim(), Correo = value.Correo.Trim().ToLowerInvariant(), RolId = RolDgaa };
        _db.Usuarios.Add(user);
        await _db.SaveChangesAsync();
        _db.UsuariosDgaa.Add(new SGPla.Data.NewModel.Entities.UsuarioDgaa { UsuarioId = user.Id, AreaAcademicaId = value.IdAreaAcademica });
        await _db.SaveChangesAsync();
        return user.Id;
    }

    public Task<bool> ExisteCorreoAsync(string correo) =>
        _db.Usuarios.AsNoTracking().AnyAsync(x => x.Correo == correo && x.RolId == RolDgaa && x.FechaEliminacion == null);

    public async Task<CoordinadorDgaa?> ObtenerPorIdAsync(int id) => (await Buscar(x => x.UserId == id)).FirstOrDefault();

    public async Task<CoordinadorDgaa?> ObtenerPorCorreoAsync(string correo) => (await Buscar(x => x.Correo == correo)).FirstOrDefault();

    public Task<List<CoordinadorDgaa>> ObtenerTodosAsync() => Buscar(_ => true);

    public Task<List<CoordinadorDgaa>> BuscarConFiltros(int? idAreaAcademica, string? busqueda) =>
        Buscar(x => (!idAreaAcademica.HasValue || x.AreaId == idAreaAcademica.Value) &&
            (string.IsNullOrWhiteSpace(busqueda) || x.Nombre.Contains(busqueda.Trim()) || x.Correo.Contains(busqueda.Trim()) || (x.Cargo ?? string.Empty).Contains(busqueda.Trim())));

    public async Task ActualizarAsync(CoordinadorDgaa value)
    {
        var user = await _db.Usuarios.FirstOrDefaultAsync(x => x.Id == value.IdCoordinadorDgaa && x.RolId == RolDgaa && x.FechaEliminacion == null)
            ?? throw new InvalidOperationException("No se encontró el coordinador DGAA.");
        user.Nombre = value.Nombre.Trim();
        var link = await _db.UsuariosDgaa.FirstAsync(x => x.UsuarioId == user.Id);
        link.AreaAcademicaId = value.IdAreaAcademica;
        _db.Entry(user).State = EntityState.Modified;
        _db.Entry(link).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    public async Task EliminarAsync(CoordinadorDgaa value)
    {
        var user = await _db.Usuarios.FirstOrDefaultAsync(x => x.Id == value.IdCoordinadorDgaa && x.RolId == RolDgaa && x.FechaEliminacion == null);
        if (user is null) return;
        user.FechaEliminacion = DateTime.UtcNow;
        _db.Entry(user).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    public async Task<SuperUsuario?> ObtenerSuperUsuarioPorCorreoAsync(string correo)
    {
        var user = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Correo == correo && x.RolId == 1 && x.FechaEliminacion == null);
        return user is null ? null : new SuperUsuario { IdSuperUsuario = user.Id, Nombre = user.Nombre, Correo = user.Correo };
    }

    private async Task<List<CoordinadorDgaa>> Buscar(Func<Row, bool> predicate)
    {
        var rows = await (from link in _db.UsuariosDgaa.AsNoTracking()
                           join user in _db.Usuarios.AsNoTracking() on link.UsuarioId equals user.Id
                           join area in _db.AreaAcademicas.AsNoTracking() on link.AreaAcademicaId equals area.Id
                           where user.RolId == RolDgaa && user.FechaEliminacion == null && area.FechaEliminacion == null
                           select new Row(user.Id, user.Nombre, user.Correo, link.AreaAcademicaId, area.Nombre)).ToListAsync();
        return rows.Where(predicate).Select(x => new CoordinadorDgaa
        {
            IdCoordinadorDgaa = x.UserId,
            IdAreaAcademica = x.AreaId,
            Nombre = x.Nombre,
            Correo = x.Correo,
            Cargo = string.Empty,
            IdAreaAcademicaNavigation = new AreaAcademica { IdAreaAcademica = x.AreaId, Nombre = x.AreaNombre, Telefono = string.Empty }
        }).ToList();
    }

    private sealed record Row(int UserId, string Nombre, string Correo, int AreaId, string AreaNombre)
    {
        public string? Cargo => null;
    }
}
