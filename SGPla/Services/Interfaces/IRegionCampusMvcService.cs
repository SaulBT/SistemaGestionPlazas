using SGPla.Models.DTOs.Catalogos;

namespace SGPla.Services.Interfaces;

public interface IRegionCampusMvcService
{
    Task<(IReadOnlyList<RegionAdministracionFila> Regiones, IReadOnlyList<CampusAdministracionFila> Campus)> ObtenerAsync(CancellationToken cancellationToken = default);
    Task CrearRegionAsync(CrearRegionMvcDto dto, CancellationToken cancellationToken = default);
    Task<bool> EditarNombreRegionAsync(int id, string nombre, CancellationToken cancellationToken = default);
    Task<bool> DarDeBajaRegionAsync(int id, CancellationToken cancellationToken = default);
    Task CrearCampusAsync(CrearCampusMvcDto dto, CancellationToken cancellationToken = default);
    Task<bool> EditarNombreCampusAsync(int id, string nombre, CancellationToken cancellationToken = default);
    Task<bool> DarDeBajaCampusAsync(int id, CancellationToken cancellationToken = default);
}
