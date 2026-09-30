using SGPla.Models.DTOs.Planea;

namespace SGPla.Services.Interfaces
{
    public interface IPlaneaCliente
    {
        Task<PlaneaRespuesta> ObtenerPeriodoAsync(string codigoPeriodo, CancellationToken cancellationToken = default);
    }
}
