using SGPla.Models.ViewModels.Catalogos;

namespace SGPla.Services.Interfaces;

/// <summary>
/// Catálogos del modelo normalizado usados por las vistas MVC.
/// Mantiene los combos fuera del contexto legacy de REST.
/// </summary>
public interface ICatalogosMvcService
{
    Task<CatalogosViewModel> ObtenerAsync(CancellationToken cancellationToken = default);
    Task<CatalogosEntidadAcademicaViewModel> ObtenerCatalogosEntidadAcademicaAsync(int? regionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogoOpcion>> ObtenerCampusAsync(int? regionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogoOpcion>> ObtenerEntidadesAsync(int? campusId, int? areaAcademicaId, CancellationToken cancellationToken = default, int? regionId = null);
    Task<int?> ObtenerRegionEntidadAsync(int entidadAcademicaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogoOpcion>> ObtenerMunicipiosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogoOpcion>> ObtenerProgramasAsync(int? entidadAcademicaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogoOpcion>> ObtenerPlanesAsync(int? programaEducativoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogoOpcion>> ObtenerExperienciasAsync(int? planEstudiosId, CancellationToken cancellationToken = default);
}
