using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class EntidadAcademicaMvcService : IEntidadAcademicaMvcService
{
    private readonly IEntidadAcademicaMvcRepository _repository;
    private readonly TimeProvider _timeProvider;

    public EntidadAcademicaMvcService(IEntidadAcademicaMvcRepository repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public Task<(IReadOnlyList<EntidadAcademicaMvcDto> Items, int TotalCount)> BuscarAsync(
        FiltroEntidadAcademicaMvcDto filtro, CancellationToken cancellationToken = default)
    {
        if (filtro.Pagina < 1 || filtro.Cantidad is < 1 or > 100)
            throw new ArgumentException("La página o la cantidad solicitada no son válidas.");
        if (filtro.RegionId is <= 0 || filtro.AreaAcademicaId is <= 0)
            throw new ArgumentException("Los filtros de catálogo deben ser identificadores válidos.");
        return _repository.BuscarAsync(filtro, cancellationToken);
    }

    public Task<EntidadAcademicaMvcDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0) throw new ArgumentException("La entidad académica no es válida.");
        return _repository.ObtenerPorIdAsync(id, cancellationToken);
    }

    public async Task<int> CrearAsync(EntidadAcademicaMvcInputDto datos, CancellationToken cancellationToken = default)
    {
        ValidarDatos(datos, incluirClave: true);
        if (!await _repository.ReferenciasActivasAsync(datos, cancellationToken))
            throw new ArgumentException("Seleccione un campus, área académica y municipio activos.");

        try
        {
            return await _repository.CrearAsync(datos, cancellationToken);
        }
        catch (DbUpdateException exception) when (EsClaveDuplicada(exception))
        {
            throw new InvalidOperationException("La clave de entidad académica ya está registrada.", exception);
        }
        catch (DbUpdateException exception) when (EsReferenciaInvalida(exception))
        {
            throw new InvalidOperationException("Uno de los catálogos seleccionados dejó de estar disponible. Recargue el formulario.", exception);
        }
    }

    public async Task ActualizarAsync(int id, EntidadAcademicaMvcInputDto datos, CancellationToken cancellationToken = default)
    {
        ValidarDatos(datos, incluirClave: false);
        var actual = await _repository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("La entidad académica no existe o está inactiva.");

        if (datos.CampusId != actual.CampusId || !string.Equals(datos.Clave.Trim(), actual.Clave, StringComparison.Ordinal))
            throw new InvalidOperationException("La clave y el campus de una entidad académica son inmutables.");
        if (datos.AreaAcademicaId != actual.AreaAcademicaId && await _repository.TieneProgramasAsync(id, cancellationToken))
            throw new InvalidOperationException("El área académica no puede cambiar porque la entidad ya tiene programas educativos.");
        if (!await _repository.ReferenciasActivasAsync(datos, cancellationToken))
            throw new ArgumentException("Seleccione un área académica y municipio activos.");

        try
        {
            await _repository.ActualizarAsync(id, datos, cancellationToken);
        }
        catch (DbUpdateException exception) when (EsReferenciaInvalida(exception))
        {
            throw new InvalidOperationException("Uno de los catálogos seleccionados dejó de estar disponible. Recargue el formulario.", exception);
        }
    }

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0) throw new ArgumentException("La entidad académica no es válida.");
        var fechaUtc = _timeProvider.GetUtcNow().UtcDateTime;
        try
        {
            await _repository.EliminarEnCascadaAsync(id, fechaUtc, cancellationToken);
        }
        catch (SqlException exception) when (exception.Number == 547)
        {
            throw new InvalidOperationException("No se puede dar de baja la entidad por sus referencias activas.", exception);
        }
    }

    private static void ValidarDatos(EntidadAcademicaMvcInputDto datos, bool incluirClave)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (incluirClave && (string.IsNullOrWhiteSpace(datos.Clave) || datos.Clave.Length > 50 ||
            datos.Clave != datos.Clave.Trim().ToUpperInvariant() || !datos.Clave.All(IsAsciiLetterOrDigit)))
            throw new ArgumentException("La clave es obligatoria y debe usar solo letras mayúsculas y números.");
        ValidarTexto(datos.Nombre, 200, "El nombre");
        ValidarTexto(datos.Calle, 200, "La calle");
        ValidarTexto(datos.Colonia, 150, "La colonia");
        if (datos.NumeroExterior is { Length: > 20 })
            throw new ArgumentException("El número exterior no puede exceder 20 caracteres.");
        if (string.IsNullOrWhiteSpace(datos.CodigoPostal) || datos.CodigoPostal.Length != 5 || !datos.CodigoPostal.All(IsAsciiDigit))
            throw new ArgumentException("El código postal debe tener exactamente cinco dígitos.");
        if (string.IsNullOrWhiteSpace(datos.Telefono) || datos.Telefono.Length != 10 || !datos.Telefono.All(IsAsciiDigit))
            throw new ArgumentException("El teléfono debe tener exactamente diez dígitos.");
        if (datos.Extension is { Length: > 10 } ||
            (!string.IsNullOrWhiteSpace(datos.Extension) && !datos.Extension.All(IsAsciiDigit)))
            throw new ArgumentException("La extensión debe contener de uno a diez dígitos.");
        if (datos.CampusId <= 0 || datos.AreaAcademicaId <= 0 || datos.MunicipioId <= 0)
            throw new ArgumentException("Seleccione valores válidos para campus, área académica y municipio.");
    }

    private static void ValidarTexto(string? value, int maxLength, string label)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength)
            throw new ArgumentException($"{label} es obligatorio y no puede exceder {maxLength} caracteres.");
    }

    private static bool EsClaveDuplicada(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };

    private static bool EsReferenciaInvalida(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 547 };

    private static bool IsAsciiDigit(char value) => value is >= '0' and <= '9';
    private static bool IsAsciiLetterOrDigit(char value) => IsAsciiDigit(value) || value is >= 'A' and <= 'Z';
}
