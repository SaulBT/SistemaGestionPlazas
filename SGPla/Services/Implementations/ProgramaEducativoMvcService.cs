using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Models.ViewModels.Catalogos;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class ProgramaEducativoMvcService : IProgramaEducativoMvcService
{
    private readonly IProgramaEducativoMvcRepository _repository;
    private readonly ICatalogosMvcService _catalogos;
    private readonly TimeProvider _timeProvider;

    public ProgramaEducativoMvcService(IProgramaEducativoMvcRepository repository, ICatalogosMvcService catalogos, TimeProvider timeProvider)
    { _repository = repository; _catalogos = catalogos; _timeProvider = timeProvider; }

    public Task<PaginaProgramasEducativosMvc> BuscarAsync(ProgramaEducativoMvcFiltro filtro, CancellationToken cancellationToken = default) =>
        _repository.BuscarAsync(filtro, cancellationToken);

    public Task<ProgramaEducativoMvcDto?> ObtenerAsync(int id, CancellationToken cancellationToken = default) =>
        _repository.ObtenerAsync(id, cancellationToken);

    public async Task<int> GuardarAsync(GuardarProgramaEducativoMvcDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Nombre.Trim().Length > 200)
            throw new ArgumentException("El nombre es obligatorio y no debe exceder 200 caracteres.");
        if (!await _repository.CatalogosActivosAsync(dto.EntidadAcademicaId, dto.SistemaEducativoId, dto.NivelFormacionId, cancellationToken))
            throw new ArgumentException("Seleccione una entidad, sistema educativo y nivel de formación activos.");
        try
        {
            if (dto.Id.HasValue)
            {
                var actual = await _repository.ObtenerAsync(dto.Id.Value, cancellationToken)
                    ?? throw new KeyNotFoundException("El programa educativo no existe o está inactivo.");
                if (actual.EntidadAcademicaId != dto.EntidadAcademicaId)
                    throw new InvalidOperationException("La Entidad Académica de un programa educativo es inmutable.");
                if ((actual.SistemaEducativoId != dto.SistemaEducativoId || actual.NivelFormacionId != dto.NivelFormacionId) &&
                    await _repository.TienePlanesAsync(dto.Id.Value, cancellationToken))
                    throw new InvalidOperationException("El sistema educativo y el nivel de formación no pueden cambiar porque el programa ya tuvo planes de estudio.");
                if (!await _repository.ActualizarAsync(dto, cancellationToken)) throw new KeyNotFoundException("El programa educativo no existe o está inactivo.");
                return dto.Id.Value;
            }
            return await _repository.CrearAsync(dto, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 } sqlException)
        {
            throw new InvalidOperationException("Ya existe un programa con ese nombre, entidad y sistema educativo.", sqlException);
        }
    }

    public Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default) =>
        _repository.EliminarAsync(id, _timeProvider.GetUtcNow().UtcDateTime, cancellationToken);

    public async Task<IReadOnlyList<CatalogoOpcion>> ObtenerEntidadesAsync(int? regionId, int? areaAcademicaId, CancellationToken cancellationToken = default) =>
        await _catalogos.ObtenerEntidadesAsync(null, areaAcademicaId, cancellationToken, regionId);
}
