using Microsoft.Extensions.Options;
using SGPla.Commons;
using SGPla.Models.DTOs.Planea;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations
{
    public sealed class SincronizarPeriodosVigentesService(ISincronizacionPlaneaRepository repositorio,
        ISincronizarPeriodoPlaneaService sincronizarPeriodo, IOptions<PlaneaOpciones> opciones,
        ILogger<SincronizarPeriodosVigentesService> logger) : ISincronizarPeriodosVigentesService
    {
        public async Task<IReadOnlyList<ResultadoSincronizacionPlanea>> EjecutarAsync(CancellationToken cancellationToken = default)
        {
            await repositorio.MarcarInterrumpidasAsync(opciones.Value.UmbralInterrumpida, cancellationToken);
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var periodos = await repositorio.ObtenerPeriodosVigentesAsync(hoy, opciones.Value.VentanaAnticipacionMeses, cancellationToken);
            var sinFechas = await repositorio.ContarPeriodosSinFechasAsync(cancellationToken);
            if (sinFechas > 0) logger.LogInformation("Hay {Cantidad} periodos sin fechas registradas; se usan las fechas derivadas de su código.", sinFechas);
            if (periodos.Count == 0) { logger.LogInformation("No hay periodos vigentes para sincronizar con PLANEA."); return []; }
            var resultados = new List<ResultadoSincronizacionPlanea>(periodos.Count);
            foreach (var periodo in periodos) resultados.Add(await sincronizarPeriodo.SincronizarAsync(periodo, cancellationToken));
            return resultados;
        }
    }
}
