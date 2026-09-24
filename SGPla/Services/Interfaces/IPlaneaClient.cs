using SGPla.Models.DTOs.Integracion;

namespace SGPla.Services.Interfaces;

public interface IPlaneaClient
{
    Task<IReadOnlyList<PlaneaRegistro>> ObtenerProgramacionesAsync(string clavePeriodo, CancellationToken cancellationToken = default);
}
