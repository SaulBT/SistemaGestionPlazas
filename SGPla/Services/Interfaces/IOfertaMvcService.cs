using SGPla.Models.DTOs.Ofertas;

namespace SGPla.Services.Interfaces;

public interface IOfertaMvcService
{
    Task<OfertaMvcNuevaDatos?> ObtenerDatosNuevaAsync(int programacionAcademicaId, int entidadAcademicaId, CancellationToken cancellationToken = default);
    Task<int> CrearAsync(CrearOfertaMvcDatos datos, int entidadAcademicaId, CancellationToken cancellationToken = default);
}
