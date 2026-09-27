using System.Data;
using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Models.DTOs.Auth;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class SuperusuarioAdminService : ISuperusuarioAdminService
{
    private const byte RolSuperusuario = 1;
    private readonly SgplaDbContext _db;
    private readonly IArgon2idPasswordHasher _hasher;
    private readonly TimeProvider _timeProvider;

    public SuperusuarioAdminService(SgplaDbContext db, IArgon2idPasswordHasher hasher, TimeProvider timeProvider)
    {
        _db = db;
        _hasher = hasher;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<SuperusuarioAdministrable>> ListarAsync(int actorId,
        CancellationToken cancellationToken = default)
    {
        await ValidarActorAsync(actorId, cancellationToken);
        return await _db.Usuarios.AsNoTracking()
            .Where(x => x.RolId == RolSuperusuario && x.FechaEliminacion == null)
            .OrderBy(x => x.Nombre).ThenBy(x => x.Id)
            .Select(x => new SuperusuarioAdministrable(x.Id, x.Nombre, x.Correo,
                _db.CredencialSuperusuarios.AsNoTracking().Any(c => c.UsuarioId == x.Id && c.FechaEliminacion == null)))
            .ToListAsync(cancellationToken);
    }

    public async Task RestablecerContrasenaTemporalAsync(int actorId, int usuarioId, string contrasenaTemporal,
        CancellationToken cancellationToken = default)
    {
        if (usuarioId < 1) throw new ArgumentException("Selecciona un Superusuario vigente.", nameof(usuarioId));
        if (!PoliticaContrasenaSuperusuario.EsValida(contrasenaTemporal))
            throw new ArgumentException("La contraseña temporal debe tener entre 8 y 128 caracteres, mayúscula, minúscula, número y símbolo.", nameof(contrasenaTemporal));

        var hash = _hasher.Hash(contrasenaTemporal);
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await ValidarActorAsync(actorId, cancellationToken);
        var cuentaActiva = await _db.Usuarios.AsNoTracking().AnyAsync(x => x.Id == usuarioId
            && x.RolId == RolSuperusuario && x.FechaEliminacion == null, cancellationToken);
        if (!cuentaActiva) throw new KeyNotFoundException("No se encontró el Superusuario activo.");

        var actualizadas = await _db.CredencialSuperusuarios.Where(x => x.UsuarioId == usuarioId && x.FechaEliminacion == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Contrasena, hash)
                .SetProperty(x => x.FechaActualizacion, (DateTime?)null), cancellationToken);
        if (actualizadas != 1)
            throw new InvalidOperationException("El Superusuario no tiene una credencial local vigente para restablecer.");
        await transaction.CommitAsync(CancellationToken.None);
    }

    public async Task DesactivarAsync(int actorId, int usuarioId, CancellationToken cancellationToken = default)
    {
        if (usuarioId < 1) throw new ArgumentException("Selecciona un Superusuario vigente.", nameof(usuarioId));
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await ValidarActorAsync(actorId, cancellationToken);
        var cuentaActiva = await _db.Usuarios.AsNoTracking().AnyAsync(x => x.Id == usuarioId
            && x.RolId == RolSuperusuario && x.FechaEliminacion == null, cancellationToken);
        if (!cuentaActiva) throw new KeyNotFoundException("No se encontró el Superusuario activo.");

        var cuentasActivas = await _db.Usuarios.AsNoTracking()
            .CountAsync(x => x.RolId == RolSuperusuario && x.FechaEliminacion == null, cancellationToken);
        if (cuentasActivas <= 1)
            throw new InvalidOperationException("No se puede desactivar al último Superusuario activo.");

        var ahora = _timeProvider.GetUtcNow().UtcDateTime;
        await _db.CredencialSuperusuarios.Where(x => x.UsuarioId == usuarioId && x.FechaEliminacion == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.FechaEliminacion, ahora), cancellationToken);
        var desactivados = await _db.Usuarios.Where(x => x.Id == usuarioId
                && x.RolId == RolSuperusuario && x.FechaEliminacion == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.FechaEliminacion, ahora), cancellationToken);
        if (desactivados != 1) throw new InvalidOperationException("El Superusuario cambió durante la desactivación.");
        await transaction.CommitAsync(CancellationToken.None);
    }

    private async Task ValidarActorAsync(int actorId, CancellationToken cancellationToken)
    {
        if (actorId < 1 || !await _db.Usuarios.AsNoTracking().AnyAsync(x => x.Id == actorId
                && x.RolId == RolSuperusuario && x.FechaEliminacion == null, cancellationToken))
            throw new UnauthorizedAccessException("La sesión no pertenece a un Superusuario activo.");
    }
}
