using Microsoft.EntityFrameworkCore;
using SGPla.Commons;
using SGPla.Data.NewModel;
using SGPla.Models.DTOs.Usuarios;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

public sealed class NormalizedUsuarioConsultaRepository : IUsuarioConsultaRepository
{
    private const byte RolDgaaId = 2;
    private const byte RolEntidadId = 3;
    private readonly SgplaDbContext _db;

    public NormalizedUsuarioConsultaRepository(SgplaDbContext db) => _db = db;

    public async Task<(List<ListaUsuarioDTO> Items, int TotalCount)> BuscarPaginadoAsync(
        FiltrosUsuarioDTO filtro,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        if (filtro.Pagina < 1 || filtro.Cantidad is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(filtro), "La página o cantidad solicitada no son válidas.");

        int? regionClave = null;
        if (!string.IsNullOrWhiteSpace(filtro.Region))
        {
            var separator = filtro.Region.IndexOf('-');
            var clave = separator > 0 ? filtro.Region[..separator] : filtro.Region;
            if (!int.TryParse(clave, out var parsedClave))
                return ([], 0);
            regionClave = parsedClave;
        }

        var consultaEa =
            from perfil in _db.UsuariosEntidadAcademica.AsNoTracking()
            join usuario in _db.Usuarios.AsNoTracking() on perfil.UsuarioId equals usuario.Id
            join entidad in _db.EntidadAcademicas.AsNoTracking() on perfil.EntidadAcademicaId equals entidad.Id
            join area in _db.AreaAcademicas.AsNoTracking() on entidad.AreaAcademicaId equals area.Id
            join campus in _db.Campuses.AsNoTracking() on entidad.CampusId equals campus.Id
            join region in _db.Regiones.AsNoTracking() on campus.RegionId equals region.Id
            where usuario.RolId == RolEntidadId && usuario.FechaEliminacion == null &&
                  entidad.FechaEliminacion == null && area.FechaEliminacion == null && campus.FechaEliminacion == null &&
                  region.FechaEliminacion == null
            select new
            {
                IdUsuario = usuario.Id,
                usuario.Nombre,
                usuario.Correo,
                RolId = (byte)RolEntidadId,
                EntidadId = (int?)entidad.Id,
                EntidadNombre = (string?)entidad.Nombre,
                AreaId = area.Id,
                AreaNombre = area.Nombre,
                RegionClave = (int?)region.Clave,
                RegionNombre = (string?)region.Nombre
            };

        if (filtro.IdEntidadAcademica.HasValue)
            consultaEa = consultaEa.Where(x => x.EntidadId == filtro.IdEntidadAcademica.Value);
        if (filtro.IdAreaAcademica.HasValue)
            consultaEa = consultaEa.Where(x => x.AreaId == filtro.IdAreaAcademica.Value);
        if (regionClave.HasValue)
            consultaEa = consultaEa.Where(x => x.RegionClave == regionClave.Value);

        var consultaDgaa =
            from perfil in _db.UsuariosDgaa.AsNoTracking()
            join usuario in _db.Usuarios.AsNoTracking() on perfil.UsuarioId equals usuario.Id
            join area in _db.AreaAcademicas.AsNoTracking() on perfil.AreaAcademicaId equals area.Id
            where usuario.RolId == RolDgaaId && usuario.FechaEliminacion == null && area.FechaEliminacion == null
            select new
            {
                IdUsuario = usuario.Id,
                usuario.Nombre,
                usuario.Correo,
                RolId = (byte)RolDgaaId,
                EntidadId = (int?)null,
                EntidadNombre = (string?)null,
                AreaId = area.Id,
                AreaNombre = area.Nombre,
                RegionClave = (int?)null,
                RegionNombre = (string?)null
            };

        if (filtro.IdAreaAcademica.HasValue)
            consultaDgaa = consultaDgaa.Where(x => x.AreaId == filtro.IdAreaAcademica.Value);

        var busqueda = filtro.Busqueda?.Trim();
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            consultaEa = consultaEa.Where(x => x.Nombre.Contains(busqueda) || x.Correo.Contains(busqueda));
            consultaDgaa = consultaDgaa.Where(x => x.Nombre.Contains(busqueda) || x.Correo.Contains(busqueda));
        }

        var consulta = filtro.Rol switch
        {
            Constantes.COORDINADOR_EA => consultaEa,
            Constantes.COORDINADOR_DGAA => consultaDgaa,
            null or "" => consultaEa.Concat(consultaDgaa),
            _ => consultaEa.Where(_ => false)
        };

        var total = await consulta.CountAsync(cancellationToken);
        var items = await consulta
            .OrderBy(x => x.Nombre)
            .ThenBy(x => x.IdUsuario)
            .ThenBy(x => x.RolId)
            .Skip((filtro.Pagina - 1) * filtro.Cantidad)
            .Take(filtro.Cantidad)
            .Select(x => new ListaUsuarioDTO
            {
                IdUsuario = x.IdUsuario,
                Nombre = x.Nombre,
                Correo = x.Correo,
                Cargo = string.Empty,
                Rol = x.RolId == RolDgaaId ? Constantes.COORDINADOR_DGAA : Constantes.COORDINADOR_EA,
                NombreEntidadAcademica = x.EntidadNombre,
                NombreAreaAcademica = x.AreaNombre,
                Region = x.RegionClave.HasValue ? x.RegionClave.Value.ToString() + "-" + x.RegionNombre : null
            })
            .ToListAsync(cancellationToken);
        return (items, total);
    }

}
