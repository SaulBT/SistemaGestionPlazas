using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SGPla.Commons;
using SGPla.Data.NewModel;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class NormalizedUsuarioSesionValidator : IUsuarioSesionValidator
{
    private readonly SgplaDbContext _db;

    public NormalizedUsuarioSesionValidator(SgplaDbContext db) => _db = db;

    public async Task<bool> EsSesionVigenteAsync(ClaimsPrincipal? principal, CancellationToken cancellationToken = default)
    {
        if (principal?.Identity?.IsAuthenticated != true ||
            !int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId) || usuarioId <= 0)
            return false;

        var rol = principal.FindFirstValue(ClaimTypes.Role);
        var rolId = rol switch
        {
            Constantes.SUPERUSUARIO => (byte)1,
            Constantes.COORDINADOR_DGAA => (byte)2,
            Constantes.COORDINADOR_EA => (byte)3,
            _ => (byte)0
        };
        if (rolId == 0)
            return false;

        var usuarioActivo = await _db.Usuarios.AsNoTracking()
            .AnyAsync(x => x.Id == usuarioId && x.RolId == rolId && x.FechaEliminacion == null, cancellationToken);
        if (!usuarioActivo)
            return false;

        return rolId switch
        {
            1 => await EsSesionSuperusuarioVigenteAsync(principal, usuarioId, cancellationToken),
            2 => await EsSesionDgaaVigenteAsync(principal, usuarioId, cancellationToken),
            3 => await EsSesionEntidadVigenteAsync(principal, usuarioId, cancellationToken),
            _ => false
        };
    }

    private async Task<bool> EsSesionSuperusuarioVigenteAsync(ClaimsPrincipal principal, int usuarioId,
        CancellationToken cancellationToken)
    {
        if (principal.HasClaim(c => c.Type == Constantes.ID_AREA_ACADEMICA || c.Type == "EntidadAcademicaId") ||
            !bool.TryParse(principal.FindFirstValue("DebeCambiarContrasena"), out var debeCambiarClaim) ||
            await _db.UsuariosDgaa.AsNoTracking().AnyAsync(x => x.UsuarioId == usuarioId, cancellationToken) ||
            await _db.UsuariosEntidadAcademica.AsNoTracking().AnyAsync(x => x.UsuarioId == usuarioId, cancellationToken))
            return false;

        var debeCambiarPersistido = await _db.CredencialSuperusuarios.AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId && x.FechaEliminacion == null)
            .Select(x => (bool?)(x.FechaActualizacion == null))
            .SingleOrDefaultAsync(cancellationToken);
        return debeCambiarPersistido.HasValue && debeCambiarPersistido.Value == debeCambiarClaim;
    }

    private async Task<bool> EsSesionDgaaVigenteAsync(ClaimsPrincipal principal, int usuarioId, CancellationToken cancellationToken)
    {
        if (principal.HasClaim(c => c.Type == "EntidadAcademicaId") ||
            !int.TryParse(principal.FindFirstValue(Constantes.ID_AREA_ACADEMICA), out var areaId) || areaId <= 0)
            return false;

        return await (from perfil in _db.UsuariosDgaa.AsNoTracking()
                      join area in _db.AreaAcademicas.AsNoTracking() on perfil.AreaAcademicaId equals area.Id
                      where perfil.UsuarioId == usuarioId && perfil.AreaAcademicaId == areaId && area.FechaEliminacion == null
                      select perfil.UsuarioId).AnyAsync(cancellationToken);
    }

    private async Task<bool> EsSesionEntidadVigenteAsync(ClaimsPrincipal principal, int usuarioId, CancellationToken cancellationToken)
    {
        if (principal.HasClaim(c => c.Type == Constantes.ID_AREA_ACADEMICA) ||
            !int.TryParse(principal.FindFirstValue("EntidadAcademicaId"), out var entidadId) || entidadId <= 0)
            return false;

        return await (from perfil in _db.UsuariosEntidadAcademica.AsNoTracking()
                      join entidad in _db.EntidadAcademicas.AsNoTracking() on perfil.EntidadAcademicaId equals entidad.Id
                      join area in _db.AreaAcademicas.AsNoTracking() on entidad.AreaAcademicaId equals area.Id
                      join campus in _db.Campuses.AsNoTracking() on entidad.CampusId equals campus.Id
                      join region in _db.Regiones.AsNoTracking() on campus.RegionId equals region.Id
                      where perfil.UsuarioId == usuarioId && perfil.EntidadAcademicaId == entidadId &&
                            entidad.FechaEliminacion == null && area.FechaEliminacion == null &&
                            campus.FechaEliminacion == null && region.FechaEliminacion == null
                      select perfil.UsuarioId).AnyAsync(cancellationToken);
    }
}
