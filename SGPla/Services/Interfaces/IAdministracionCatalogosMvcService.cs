using SGPla.Models.DTOs.Catalogos;

namespace SGPla.Services.Interfaces;

public interface IAdministracionCatalogosMvcService
{
    Task<IReadOnlyList<CatalogoAdministracionMvcFila>> ListarAsync(CancellationToken cancellationToken = default);
    Task CrearClasificacionAsync(CrearCatalogoClasificacionMvcDto datos, CancellationToken cancellationToken = default);
    Task EditarNombreAsync(EditarNombreCatalogoMvcDto datos, CancellationToken cancellationToken = default);
    Task CambiarActivoAsync(TipoCatalogoNormalizado tipo, int id, bool activo, CancellationToken cancellationToken = default);
}
