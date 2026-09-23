using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Models;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

public sealed class NormalizedCoordinadorEaRepository : ICoordinadorEaRepository
{
    private const byte RolEntidad = 3;
    private readonly SgplaDbContext _db;
    public NormalizedCoordinadorEaRepository(SgplaDbContext db) => _db = db;

    public async Task<int> CrearAsync(CoordinadorEa value)
    {
        var user = new SGPla.Data.NewModel.Entities.Usuario { Nombre = value.Nombre.Trim(), Correo = value.Correo.Trim().ToLowerInvariant(), RolId = RolEntidad };
        _db.Usuarios.Add(user);
        await _db.SaveChangesAsync();
        _db.UsuariosEntidadAcademica.Add(new SGPla.Data.NewModel.Entities.UsuarioEntidadAcademica { UsuarioId = user.Id, EntidadAcademicaId = value.IdEntidadAcademica });
        await _db.SaveChangesAsync();
        return user.Id;
    }

    public Task<bool> ExisteCorreoAsync(string correo) =>
        _db.Usuarios.AsNoTracking().AnyAsync(x => x.Correo == correo && x.RolId == RolEntidad && x.FechaEliminacion == null);

    public async Task<CoordinadorEa?> ObtenerPorIdAsync(int id) => (await Buscar(x => x.UserId == id)).FirstOrDefault();
    public async Task<CoordinadorEa?> ObtenerPorCorreoAsync(string correo) => (await Buscar(x => x.Correo == correo)).FirstOrDefault();
    public Task<List<CoordinadorEa>> ObtenerTodosAsync() => Buscar(_ => true);

    public Task<List<CoordinadorEa>> BuscarConFiltros(string? region, int? idAreaAcademica, int? idEntidadAcademica, string? busqueda) =>
        Buscar(x => (!idAreaAcademica.HasValue || x.AreaId == idAreaAcademica.Value) &&
            (!idEntidadAcademica.HasValue || x.EntityId == idEntidadAcademica.Value) &&
            (string.IsNullOrWhiteSpace(region) || x.Region == region) &&
            (string.IsNullOrWhiteSpace(busqueda) || x.Nombre.Contains(busqueda.Trim()) || x.Correo.Contains(busqueda.Trim()) || (x.Cargo ?? string.Empty).Contains(busqueda.Trim())));

    public async Task ActualizarAsync(CoordinadorEa value)
    {
        var user = await _db.Usuarios.FirstOrDefaultAsync(x => x.Id == value.IdCoordinadorEa && x.RolId == RolEntidad && x.FechaEliminacion == null)
            ?? throw new InvalidOperationException("No se encontró el coordinador de entidad académica.");
        user.Nombre = value.Nombre.Trim();
        var link = await _db.UsuariosEntidadAcademica.FirstAsync(x => x.UsuarioId == user.Id);
        link.EntidadAcademicaId = value.IdEntidadAcademica;
        _db.Entry(user).State = EntityState.Modified;
        _db.Entry(link).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    public async Task EliminarAsync(CoordinadorEa value)
    {
        var user = await _db.Usuarios.FirstOrDefaultAsync(x => x.Id == value.IdCoordinadorEa && x.RolId == RolEntidad && x.FechaEliminacion == null);
        if (user is null) return;
        user.FechaEliminacion = DateTime.UtcNow;
        _db.Entry(user).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    private async Task<List<CoordinadorEa>> Buscar(Func<Row, bool> predicate)
    {
        var rows = await (from link in _db.UsuariosEntidadAcademica.AsNoTracking()
                           join user in _db.Usuarios.AsNoTracking() on link.UsuarioId equals user.Id
                           join entity in _db.EntidadAcademicas.AsNoTracking() on link.EntidadAcademicaId equals entity.Id
                           join area in _db.AreaAcademicas.AsNoTracking() on entity.AreaAcademicaId equals area.Id
                           join campus in _db.Campuses.AsNoTracking() on entity.CampusId equals campus.Id
                           join region in _db.Regiones.AsNoTracking() on campus.RegionId equals region.Id
                           where user.RolId == RolEntidad && user.FechaEliminacion == null && entity.FechaEliminacion == null && area.FechaEliminacion == null && campus.FechaEliminacion == null
                           select new Row(user.Id, user.Nombre, user.Correo, link.EntidadAcademicaId, entity.AreaAcademicaId, entity.Nombre, entity.Clave, region.Clave, region.Nombre, area.Nombre)).ToListAsync();
        return rows.Where(predicate).Select(x => new CoordinadorEa
        {
            IdCoordinadorEa = x.UserId,
            IdEntidadAcademica = x.EntityId,
            Nombre = x.Nombre,
            Correo = x.Correo,
            Cargo = string.Empty,
            IdEntidadAcademicaNavigation = new EntidadAcademica
            {
                IdEntidadAcademica = x.EntityId, IdAreaAcademica = x.AreaId, Clave = x.EntityClave, Nombre = x.EntityNombre,
                Region = $"{x.RegionClave}-{x.RegionNombre}", IdAreaAcademicaNavigation = new AreaAcademica { IdAreaAcademica = x.AreaId, Nombre = x.AreaNombre, Telefono = string.Empty }
            }
        }).ToList();
    }

    private sealed record Row(int UserId, string Nombre, string Correo, int EntityId, int AreaId, string EntityNombre, string EntityClave, int RegionClave, string RegionNombre, string AreaNombre)
    {
        public string Region => $"{RegionClave}-{RegionNombre}";
        public string? Cargo => null;
    }
}
