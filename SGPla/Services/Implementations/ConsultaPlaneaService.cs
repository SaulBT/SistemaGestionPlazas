using SGPla.Commons;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Planea;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations
{
    public sealed class ConsultaPlaneaService(IConsultaPlaneaRepository repositorio) : IConsultaPlaneaService
    {
        public Task<(List<BitacoraPlaneaDTO> Items, int Total)> ObtenerBitacoraAsync(FiltroBitacoraPlaneaDTO f, CancellationToken cancellationToken = default)
        {
            f.Pagina = Math.Max(1, f.Pagina); if (f.Cantidad is < 1 or > 100) f.Cantidad = 10;
            if (f.Estado is not null && !PlaneaConstantes.ESTADOS.Contains(f.Estado, StringComparer.Ordinal)) f.Estado = null;
            return repositorio.ObtenerBitacoraAsync(f, cancellationToken);
        }
        public Task<(List<CopiaPlaneaFilaDTO> Items, int Total)> ObtenerCopiasAsync(FiltroCopiaPlaneaDTO f, CancellationToken cancellationToken = default)
        {
            f.Pagina = Math.Max(1, f.Pagina); if (f.Cantidad is < 1 or > 100) f.Cantidad = 20;
            f.Busqueda = string.IsNullOrWhiteSpace(f.Busqueda) ? null : f.Busqueda.Trim();
            if (f.Busqueda?.Length > 100) f.Busqueda = f.Busqueda[..100];
            return repositorio.ObtenerCopiasAsync(f, cancellationToken);
        }
        public Task<DetalleCopiaPlaneaDTO?> ObtenerDetalleCopiaAsync(int id, CancellationToken cancellationToken = default) => repositorio.ObtenerDetalleCopiaAsync(id, cancellationToken);
        public Task<List<OptionModel>> ObtenerPeriodosConSincronizacionAsync(CancellationToken cancellationToken = default) => repositorio.ObtenerPeriodosConSincronizacionAsync(cancellationToken);
        public Task<List<OptionModel>> ObtenerPlanesConCopiasAsync(int? idPeriodo, CancellationToken cancellationToken = default) => repositorio.ObtenerPlanesConCopiasAsync(idPeriodo, cancellationToken);
        public Task<List<OptionModel>> ObtenerRegionesAsync(CancellationToken cancellationToken = default) => repositorio.ObtenerRegionesAsync(cancellationToken);
    }
}
