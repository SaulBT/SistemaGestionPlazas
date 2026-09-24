using SGPla.Models.DTOs.Catalogos;

namespace SGPla.Repositories.Interfaces;

public interface IRegionCampusMvcRepository
{
    Task<IReadOnlyList<RegionAdministracionFila>> ListarRegionesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CampusAdministracionFila>> ListarCampusAsync(CancellationToken cancellationToken = default);
    Task<bool> RegionActivaAsync(int regionId, CancellationToken cancellationToken = default);
    Task CrearRegionAsync(CrearRegionMvcDto dto, CancellationToken cancellationToken = default);
    Task<bool> EditarNombreRegionAsync(int id, string nombre, CancellationToken cancellationToken = default);
    Task<bool> DarDeBajaRegionAsync(int id, DateTime instanteUtc, CancellationToken cancellationToken = default);
    Task CrearCampusAsync(CrearCampusMvcDto dto, CancellationToken cancellationToken = default);
    Task<bool> EditarNombreCampusAsync(int id, string nombre, CancellationToken cancellationToken = default);
    Task<bool> DarDeBajaCampusAsync(int id, DateTime instanteUtc, CancellationToken cancellationToken = default);
}
