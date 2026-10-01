using SGPla.Models.DTOs.Planea;

namespace SGPla.Services.Interfaces
{
    public interface IProgramacionPlaneaService
    {
        Task<ProgramacionPlaneaDTO> ObtenerAsync(FiltroProgramacionPlaneaDTO filtro, CancellationToken cancellationToken = default);

        Task<UltimaSincronizacionPlaneaDTO?> ObtenerUltimaSincronizacionAsync(int? idPeriodo, CancellationToken cancellationToken = default);

        /// Devuelve null si el programa educativo o el periodo no existen.
        Task<EncabezadoProgramacionPlaneaDTO?> ObtenerEncabezadoAsync(int idProgramaEducativo, int idPeriodo, CancellationToken cancellationToken = default);

        /// Sincroniza el periodo indicado o, si no se indica, los periodos vigentes. Devuelve un resumen legible.
        Task<(bool Exito, string Mensaje)> SincronizarAsync(int? idPeriodo, CancellationToken cancellationToken = default);
    }
}
