using SGPla.Models.DTOs.Ofertas;

namespace SGPla.Repositories.Interfaces;

public interface IOfertaMvcRepository
{
    Task<OfertaMvcNuevaDatos?> ObtenerDatosNuevaAsync(int programacionAcademicaId, int entidadAcademicaId, CancellationToken cancellationToken = default);
    Task<int> CrearAsync(CrearOfertaMvcDatos datos, int entidadAcademicaId, CancellationToken cancellationToken = default);
}
