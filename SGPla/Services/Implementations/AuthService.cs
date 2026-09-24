using System.Data;
using System.Text;
using SGPla.Commons;
using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Models.DTOs.Auth;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class AuthService : IAuthService
{
    private const byte RolSuperusuario = 1;
    private const byte RolDgaa = 2;
    private const byte RolEntidadAcademica = 3;
    private const string MensajeCredencialesInvalidas = "Usuario o contraseña incorrectos.";
    private static readonly UTF8Encoding Utf8Estricto = new(false, true);

    private readonly SgplaDbContext _db;
    private readonly IArgon2idPasswordHasher _hasher;
    private readonly ILdapAuthService _ldapService;
    private readonly ICoordinadorDgaaRepository _coordinadorDgaaRepository;
    private readonly ICoordinadorEaRepository _coordinadorEaRepository;
    private readonly TimeProvider _timeProvider;

    public AuthService(SgplaDbContext db, IArgon2idPasswordHasher hasher, ILdapAuthService ldapService,
        ICoordinadorDgaaRepository coordinadorDgaaRepository, ICoordinadorEaRepository coordinadorEaRepository,
        TimeProvider timeProvider)
    {
        _db = db;
        _hasher = hasher;
        _ldapService = ldapService;
        _coordinadorDgaaRepository = coordinadorDgaaRepository;
        _coordinadorEaRepository = coordinadorEaRepository;
        _timeProvider = timeProvider;
    }

    public async Task<ResultadoAutenticacion> LoginAsync(string username, string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            return ResultadoAutenticacion.Fallido(MensajeCredencialesInvalidas);

        var correo = NormalizarCorreo(username);
        var account = await _db.Usuarios.AsNoTracking()
            .Where(x => x.Correo == correo && x.FechaEliminacion == null)
            .Select(x => new { x.Id, x.Correo, x.Nombre, x.RolId })
            .SingleOrDefaultAsync(cancellationToken);

        if (account?.RolId == RolSuperusuario)
            return await AutenticarSuperusuarioAsync(account.Id, account.Correo, account.Nombre, password, cancellationToken);

        if (account?.RolId is not (RolDgaa or RolEntidadAcademica) || !_ldapService.Autenticar(correo, password))
            return ResultadoAutenticacion.Fallido(MensajeCredencialesInvalidas);

        if (account.RolId == RolDgaa)
        {
            var coordinator = await _coordinadorDgaaRepository.ObtenerPorCorreoAsync(correo);
            if (coordinator is null || coordinator.IdCoordinadorDgaa != account.Id)
                return ResultadoAutenticacion.Fallido(MensajeCredencialesInvalidas);

            return ResultadoAutenticacion.Ok(new UsuarioDTO
            {
                Id = account.Id,
                Correo = account.Correo,
                NombreCompleto = account.Nombre,
                Rol = Constantes.COORDINADOR_DGAA,
                AreaAcademicaId = coordinator.IdAreaAcademica
            });
        }

        var entityCoordinator = await _coordinadorEaRepository.ObtenerPorCorreoAsync(correo);
        if (entityCoordinator is null || entityCoordinator.IdCoordinadorEa != account.Id)
            return ResultadoAutenticacion.Fallido(MensajeCredencialesInvalidas);

        return ResultadoAutenticacion.Ok(new UsuarioDTO
        {
            Id = account.Id,
            Correo = account.Correo,
            NombreCompleto = account.Nombre,
            Rol = Constantes.COORDINADOR_EA,
            EntidadAcademicaId = entityCoordinator.IdEntidadAcademica
        });
    }

    public async Task<bool> CambiarContrasenaSuperusuarioAsync(int usuarioId, string contrasenaActual,
        string contrasenaNueva, CancellationToken cancellationToken = default)
    {
        if (usuarioId <= 0 || string.IsNullOrEmpty(contrasenaActual) || !EsContrasenaNuevaValida(contrasenaNueva))
            return false;

        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var account = await _db.Usuarios.AsNoTracking()
            .AnyAsync(x => x.Id == usuarioId && x.RolId == RolSuperusuario && x.FechaEliminacion == null, cancellationToken);
        if (!account)
            return false;

        var credential = await _db.CredencialSuperusuarios.AsTracking()
            .SingleOrDefaultAsync(x => x.UsuarioId == usuarioId && x.FechaEliminacion == null, cancellationToken);
        if (credential is null || !_hasher.Verify(contrasenaActual, credential.Contrasena).EsCorrecta)
            return false;

        credential.Contrasena = _hasher.Hash(contrasenaNueva);
        credential.FechaActualizacion = _timeProvider.GetUtcNow().UtcDateTime;
        _db.Entry(credential).State = EntityState.Modified;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private async Task<ResultadoAutenticacion> AutenticarSuperusuarioAsync(int usuarioId, string correo,
        string nombre, string contrasena, CancellationToken cancellationToken)
    {
        var credential = await _db.CredencialSuperusuarios.AsNoTracking()
            .SingleOrDefaultAsync(x => x.UsuarioId == usuarioId && x.FechaEliminacion == null, cancellationToken);
        if (credential is null)
            return ResultadoAutenticacion.Fallido(MensajeCredencialesInvalidas);

        var verification = _hasher.Verify(contrasena, credential.Contrasena);
        if (!verification.EsCorrecta)
            return ResultadoAutenticacion.Fallido(MensajeCredencialesInvalidas);

        if (verification.RequiereRehash &&
            !await ActualizarHashSiSigueVigenteAsync(usuarioId, credential.Contrasena, contrasena, cancellationToken))
            return ResultadoAutenticacion.Fallido(MensajeCredencialesInvalidas);

        return ResultadoAutenticacion.Ok(new UsuarioDTO
        {
            Id = usuarioId,
            Correo = correo,
            NombreCompleto = nombre,
            Rol = Constantes.SUPERUSUARIO,
            RequiereCambioContrasena = credential.FechaActualizacion is null
        });
    }

    private async Task<bool> ActualizarHashSiSigueVigenteAsync(int usuarioId, string hashAnterior, string password,
        CancellationToken cancellationToken)
    {
        var hashActualizado = _hasher.Hash(password);
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var cuentaActiva = await _db.Usuarios.AsNoTracking()
            .AnyAsync(x => x.Id == usuarioId && x.RolId == RolSuperusuario && x.FechaEliminacion == null, cancellationToken);
        if (!cuentaActiva)
            return false;

        var credential = await _db.CredencialSuperusuarios.AsTracking()
            .SingleOrDefaultAsync(x => x.UsuarioId == usuarioId && x.FechaEliminacion == null, cancellationToken);
        if (credential is null)
            return false;
        if (!string.Equals(credential.Contrasena, hashAnterior, StringComparison.Ordinal))
        {
            var currentVerification = _hasher.Verify(password, credential.Contrasena);
            return currentVerification.EsCorrecta;
        }

        credential.Contrasena = hashActualizado;
        // Rehash is transparent and must not satisfy a mandatory password change.
        if (credential.FechaActualizacion is not null)
            credential.FechaActualizacion = _timeProvider.GetUtcNow().UtcDateTime;
        _db.Entry(credential).State = EntityState.Modified;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private static bool EsContrasenaNuevaValida(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length is < 8 or > 128 ||
            !password.Any(char.IsUpper) || !password.Any(char.IsLower) ||
            !password.Any(char.IsDigit) || !password.Any(c => !char.IsLetterOrDigit(c)))
            return false;
        try
        {
            return Utf8Estricto.GetByteCount(password) <= 1024;
        }
        catch (EncoderFallbackException)
        {
            return false;
        }
    }

    private static string NormalizarCorreo(string username)
    {
        var correo = username.Trim();
        if (!correo.Contains('@'))
            correo = $"{correo}@uv.mx";
        return correo.ToLowerInvariant();
    }
}
