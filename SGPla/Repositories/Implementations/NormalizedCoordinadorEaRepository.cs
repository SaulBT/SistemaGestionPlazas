using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Models;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

public sealed class NormalizedCoordinadorEaRepository : ICoordinadorEaRepository
{
    private const byte RolEntidad = 3;
    private readonly SgplaDbContext _db;
    private readonly TimeProvider _timeProvider;
    public NormalizedCoordinadorEaRepository(SgplaDbContext db, TimeProvider? timeProvider = null)
    {
        _db = db;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<int> CrearAsync(CoordinadorEa value)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        await ValidarEntidadAcademicaVigenteAsync(value.IdEntidadAcademica);
        var user = new SGPla.Data.NewModel.Entities.Usuario { Nombre = value.Nombre.Trim(), Correo = value.Correo.Trim().ToLowerInvariant(), RolId = RolEntidad };
        _db.Usuarios.Add(user);
        await _db.SaveChangesAsync();
        _db.UsuariosEntidadAcademica.Add(new SGPla.Data.NewModel.Entities.UsuarioEntidadAcademica { UsuarioId = user.Id, EntidadAcademicaId = value.IdEntidadAcademica });
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return user.Id;
    }

    public Task<bool> ExisteCorreoAsync(string correo) =>
        _db.Usuarios.AsNoTracking().AnyAsync(x => x.Correo == correo && x.RolId == RolEntidad && x.FechaEliminacion == null);

    public async Task<CoordinadorEa?> ObtenerPorIdAsync(int id) => ToDto(await Consulta(usuarioId: id).FirstOrDefaultAsync());
    public async Task<CoordinadorEa?> ObtenerPorCorreoAsync(string correo) => ToDto(await Consulta(correo: correo).FirstOrDefaultAsync());
    public Task<List<CoordinadorEa>> ObtenerTodosAsync() => MaterializarAsync(Consulta());

    public Task<List<CoordinadorEa>> BuscarConFiltros(string? region, int? idAreaAcademica, int? idEntidadAcademica, string? busqueda)
    {
        int? regionClave = null;
        if (!string.IsNullOrWhiteSpace(region))
        {
            var separator = region.IndexOf('-');
            var clave = separator > 0 ? region[..separator] : region;
            if (!int.TryParse(clave, out var parsedClave))
                return Task.FromResult(new List<CoordinadorEa>());
            regionClave = parsedClave;
        }

        return MaterializarAsync(Consulta(regionClave, idAreaAcademica, idEntidadAcademica, busqueda));
    }

    public async Task ActualizarAsync(CoordinadorEa value)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        await ValidarEntidadAcademicaVigenteAsync(value.IdEntidadAcademica);
        var user = await _db.Usuarios.AsTracking().FirstOrDefaultAsync(x => x.Id == value.IdCoordinadorEa && x.RolId == RolEntidad && x.FechaEliminacion == null)
            ?? throw new InvalidOperationException("No se encontró el coordinador de entidad académica.");
        user.Nombre = value.Nombre.Trim();
        var link = await _db.UsuariosEntidadAcademica.AsTracking().FirstOrDefaultAsync(x => x.UsuarioId == user.Id)
            ?? throw new InvalidOperationException("El coordinador no tiene perfil de Entidad Académica.");
        link.EntidadAcademicaId = value.IdEntidadAcademica;
        _db.Entry(user).State = EntityState.Modified;
        _db.Entry(link).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task EliminarAsync(CoordinadorEa value)
    {
        var user = await _db.Usuarios.AsTracking().FirstOrDefaultAsync(x => x.Id == value.IdCoordinadorEa && x.RolId == RolEntidad && x.FechaEliminacion == null);
        if (user is null) return;
        user.FechaEliminacion = _timeProvider.GetUtcNow().UtcDateTime;
        _db.Entry(user).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    private async Task ValidarEntidadAcademicaVigenteAsync(int entidadAcademicaId)
    {
        var vigente = await (from entidad in _db.EntidadAcademicas.AsNoTracking()
                             join area in _db.AreaAcademicas.AsNoTracking()
                                 on entidad.AreaAcademicaId equals area.Id
                             join campus in _db.Campuses.AsNoTracking()
                                 on entidad.CampusId equals campus.Id
                             join region in _db.Regiones.AsNoTracking()
                                 on campus.RegionId equals region.Id
                             where entidad.Id == entidadAcademicaId
                                   && entidad.FechaEliminacion == null
                                   && area.FechaEliminacion == null
                                   && campus.FechaEliminacion == null
                                   && region.FechaEliminacion == null
                             select entidad.Id).AnyAsync();
        if (!vigente)
            throw new InvalidOperationException("La Entidad Académica seleccionada no está vigente.");
    }

    private IQueryable<Row> Consulta(int? regionClave = null, int? idAreaAcademica = null, int? idEntidadAcademica = null,
        string? busqueda = null, int? usuarioId = null, string? correo = null)
    {
        var query = from link in _db.UsuariosEntidadAcademica.AsNoTracking()
                    join user in _db.Usuarios.AsNoTracking() on link.UsuarioId equals user.Id
                    join entity in _db.EntidadAcademicas.AsNoTracking() on link.EntidadAcademicaId equals entity.Id
                    join area in _db.AreaAcademicas.AsNoTracking() on entity.AreaAcademicaId equals area.Id
                    join campus in _db.Campuses.AsNoTracking() on entity.CampusId equals campus.Id
                    join region in _db.Regiones.AsNoTracking() on campus.RegionId equals region.Id
                    where user.RolId == RolEntidad && user.FechaEliminacion == null && entity.FechaEliminacion == null &&
                          area.FechaEliminacion == null && campus.FechaEliminacion == null && region.FechaEliminacion == null
                    select new { link, user, entity, area, region };
        if (regionClave.HasValue) query = query.Where(x => x.region.Clave == regionClave.Value);
        if (idAreaAcademica.HasValue) query = query.Where(x => x.entity.AreaAcademicaId == idAreaAcademica.Value);
        if (idEntidadAcademica.HasValue) query = query.Where(x => x.entity.Id == idEntidadAcademica.Value);
        if (usuarioId.HasValue) query = query.Where(x => x.user.Id == usuarioId.Value);
        if (!string.IsNullOrWhiteSpace(correo)) query = query.Where(x => x.user.Correo == correo);
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim();
            query = query.Where(x => x.user.Nombre.Contains(texto) || x.user.Correo.Contains(texto));
        }
        return query.Select(x => new Row(x.user.Id, x.user.Nombre, x.user.Correo, x.link.EntidadAcademicaId,
            x.entity.AreaAcademicaId, x.entity.Nombre, x.entity.Clave, x.region.Clave, x.region.Nombre, x.area.Nombre));
    }

    private async Task<List<CoordinadorEa>> MaterializarAsync(IQueryable<Row> query) =>
        (await query.ToListAsync()).Select(x => ToDto(x)!).ToList();

    private static CoordinadorEa? ToDto(Row? x) => x is null ? null : new CoordinadorEa
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
    };

    private sealed record Row(int UserId, string Nombre, string Correo, int EntityId, int AreaId, string EntityNombre, string EntityClave, int RegionClave, string RegionNombre, string AreaNombre)
    {
        public string Region => $"{RegionClave}-{RegionNombre}";
        public string? Cargo => null;
    }
}
