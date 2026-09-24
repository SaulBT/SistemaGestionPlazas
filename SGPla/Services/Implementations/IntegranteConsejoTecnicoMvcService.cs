using SGPla.Models.DTOs.IntegranteCt;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class IntegranteConsejoTecnicoMvcService : IIntegranteConsejoTecnicoMvcService
{
    private const int MaximoNombre = 200;
    private const int MaximoCargo = 200;
    private readonly IIntegranteConsejoTecnicoMvcRepository _repository;

    public IntegranteConsejoTecnicoMvcService(IIntegranteConsejoTecnicoMvcRepository repository)
        => _repository = repository;

    public Task<IntegranteConsejoTecnicoMvcPagina> BuscarAsync(int usuarioId, int entidadAcademicaId,
        IntegranteConsejoTecnicoMvcFiltro filtro, CancellationToken cancellationToken = default)
    {
        if (usuarioId < 1 || entidadAcademicaId < 1)
            throw new UnauthorizedAccessException("La sesión no contiene un ámbito válido.");
        ArgumentNullException.ThrowIfNull(filtro);
        return _repository.BuscarAsync(usuarioId, entidadAcademicaId, filtro, cancellationToken);
    }

    public Task CrearAsync(int usuarioId, int entidadAcademicaId, IntegranteConsejoTecnicoMvcCambio cambio,
        CancellationToken cancellationToken = default)
    {
        Validar(usuarioId, entidadAcademicaId, cambio);
        return _repository.CrearAsync(usuarioId, entidadAcademicaId, Normalizar(cambio), cancellationToken);
    }

    public Task CambiarVigenciaAsync(int usuarioId, int entidadAcademicaId, int integranteId,
        IntegranteConsejoTecnicoMvcCambio cambio, CancellationToken cancellationToken = default)
    {
        Validar(usuarioId, entidadAcademicaId, cambio);
        if (integranteId < 1) throw new ArgumentException("El integrante seleccionado no es válido.");
        return _repository.CambiarVigenciaAsync(usuarioId, entidadAcademicaId, integranteId,
            Normalizar(cambio), cancellationToken);
    }

    public Task EliminarCapturaErroneaAsync(int usuarioId, int entidadAcademicaId, int integranteId,
        CancellationToken cancellationToken = default)
    {
        if (usuarioId < 1 || entidadAcademicaId < 1)
            throw new UnauthorizedAccessException("La sesión no contiene un ámbito válido.");
        if (integranteId < 1) throw new ArgumentException("El integrante seleccionado no es válido.");
        return _repository.EliminarCapturaErroneaAsync(usuarioId, entidadAcademicaId, integranteId,
            cancellationToken);
    }

    private static void Validar(int usuarioId, int entidadAcademicaId, IntegranteConsejoTecnicoMvcCambio cambio)
    {
        ArgumentNullException.ThrowIfNull(cambio);
        if (usuarioId < 1 || entidadAcademicaId < 1)
            throw new UnauthorizedAccessException("La sesión no contiene un ámbito válido.");
        if (string.IsNullOrWhiteSpace(cambio.Nombre) || cambio.Nombre.Trim().Length > MaximoNombre)
            throw new ArgumentException("El nombre es obligatorio y admite hasta 200 caracteres.");
        if (string.IsNullOrWhiteSpace(cambio.Cargo) || cambio.Cargo.Trim().Length > MaximoCargo)
            throw new ArgumentException("El cargo es obligatorio y admite hasta 200 caracteres.");
        if (cambio.TratamientoAcademicoId < 1)
            throw new ArgumentException("Selecciona un tratamiento académico vigente.");
        if (cambio.FechaInicio == default)
            throw new ArgumentException("La fecha de inicio es obligatoria.");
        if (cambio.FechaFin.HasValue && cambio.FechaFin.Value < cambio.FechaInicio)
            throw new ArgumentException("La fecha de fin debe ser igual o posterior a la fecha de inicio.");
    }

    private static IntegranteConsejoTecnicoMvcCambio Normalizar(IntegranteConsejoTecnicoMvcCambio cambio) =>
        cambio with { Nombre = cambio.Nombre.Trim(), Cargo = cambio.Cargo.Trim() };
}
