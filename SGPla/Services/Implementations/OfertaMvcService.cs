using System.Text.RegularExpressions;
using SGPla.Models.DTOs.Ofertas;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class OfertaMvcService : IOfertaMvcService
{
    private static readonly Regex ClaveValida = new("^[A-Z0-9._-]{1,100}$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private readonly IOfertaMvcRepository _repository;

    public OfertaMvcService(IOfertaMvcRepository repository) => _repository = repository;

    public Task<OfertaMvcNuevaDatos?> ObtenerDatosNuevaAsync(int programacionAcademicaId, int entidadAcademicaId,
        CancellationToken cancellationToken = default)
    {
        if (programacionAcademicaId < 1 || entidadAcademicaId < 1) return Task.FromResult<OfertaMvcNuevaDatos?>(null);
        return _repository.ObtenerDatosNuevaAsync(programacionAcademicaId, entidadAcademicaId, cancellationToken);
    }

    public Task<int> CrearAsync(CrearOfertaMvcDatos datos, int entidadAcademicaId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (entidadAcademicaId < 1 || datos.ProgramacionAcademicaId < 1)
            throw new ArgumentException("La programación solicitada no es válida.");
        var clave = datos.ClavePlaza?.Trim().ToUpperInvariant() ?? string.Empty;
        var perfil = datos.PerfilSolicitado?.Trim() ?? string.Empty;
        var justificacion = datos.Justificacion?.Trim();
        if (!ClaveValida.IsMatch(clave)) throw new ArgumentException("La clave de plaza debe contener de 1 a 100 letras, números, punto, guion o guion bajo.");
        if (perfil.Length == 0) throw new ArgumentException("El perfil solicitado es obligatorio.");
        if (datos.TipoPlazaId < 1 || datos.TipoContratacionId < 1)
            throw new ArgumentException("Selecciona un tipo de plaza y contratación vigentes.");
        if (justificacion?.Length > 1000) throw new ArgumentException("La justificación no puede exceder 1000 caracteres.");
        return _repository.CrearAsync(datos with { ClavePlaza = clave, PerfilSolicitado = perfil, Justificacion = justificacion },
            entidadAcademicaId, cancellationToken);
    }
}
