using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SGPla.Models.DTOs.Catalogos;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class RegionCampusMvcService : IRegionCampusMvcService
{
    private static readonly Regex CampusClave = new("^[A-Z0-9]+$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private readonly IRegionCampusMvcRepository _repository;
    private readonly TimeProvider _timeProvider;

    public RegionCampusMvcService(IRegionCampusMvcRepository repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<(IReadOnlyList<RegionAdministracionFila> Regiones, IReadOnlyList<CampusAdministracionFila> Campus)> ObtenerAsync(CancellationToken cancellationToken = default) =>
        (await _repository.ListarRegionesAsync(cancellationToken), await _repository.ListarCampusAsync(cancellationToken));

    public async Task CrearRegionAsync(CrearRegionMvcDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (dto.Clave <= 0) throw new ArgumentException("La clave de la región debe ser un entero positivo.");
        ValidarNombre(dto.Nombre, "región", 200);
        try { await _repository.CrearRegionAsync(dto, cancellationToken); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 } sql)
        { throw new InvalidOperationException("Ya existe una región con esa clave.", sql); }
    }

    public async Task<bool> EditarNombreRegionAsync(int id, string nombre, CancellationToken cancellationToken = default)
    {
        ValidarNombre(nombre, "región", 200);
        return id > 0 && await _repository.EditarNombreRegionAsync(id, nombre, cancellationToken);
    }

    public Task<bool> DarDeBajaRegionAsync(int id, CancellationToken cancellationToken = default) =>
        id < 1 ? Task.FromResult(false) : _repository.DarDeBajaRegionAsync(id, _timeProvider.GetUtcNow().UtcDateTime, cancellationToken);

    public async Task CrearCampusAsync(CrearCampusMvcDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var clave = dto.Clave.Trim().ToUpperInvariant();
        if (clave.Length is < 1 or > 50 || !CampusClave.IsMatch(clave)) throw new ArgumentException("La clave del Campus debe ser alfanumérica, de máximo 50 caracteres y sin espacios.");
        ValidarNombre(dto.Nombre, "Campus", 200);
        if (dto.RegionId < 1 || !await _repository.RegionActivaAsync(dto.RegionId, cancellationToken))
            throw new ArgumentException("Selecciona una región vigente.");
        try { await _repository.CrearCampusAsync(new CrearCampusMvcDto { Clave = clave, Nombre = dto.Nombre.Trim(), RegionId = dto.RegionId }, cancellationToken); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 } sql)
        { throw new InvalidOperationException("Ya existe un Campus con esa clave.", sql); }
    }

    public async Task<bool> EditarNombreCampusAsync(int id, string nombre, CancellationToken cancellationToken = default)
    {
        ValidarNombre(nombre, "Campus", 200);
        return id > 0 && await _repository.EditarNombreCampusAsync(id, nombre, cancellationToken);
    }

    public Task<bool> DarDeBajaCampusAsync(int id, CancellationToken cancellationToken = default) =>
        id < 1 ? Task.FromResult(false) : _repository.DarDeBajaCampusAsync(id, _timeProvider.GetUtcNow().UtcDateTime, cancellationToken);

    private static void ValidarNombre(string? nombre, string entidad, int maximo)
    {
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > maximo)
            throw new ArgumentException($"El nombre del/de la {entidad} es obligatorio y no debe exceder {maximo} caracteres.");
    }
}
