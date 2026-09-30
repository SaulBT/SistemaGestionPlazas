using System.Diagnostics;
using Microsoft.Extensions.Options;
using SGPla.Commons;
using SGPla.Models.DTOs.Planea;
using SGPla.Parsers;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations
{
    public sealed class SincronizarPeriodoPlaneaService(IPlaneaCliente cliente, ISincronizacionPlaneaRepository repositorio,
        IOptions<PlaneaOpciones> opciones, ILogger<SincronizarPeriodoPlaneaService> logger) : ISincronizarPeriodoPlaneaService
    {
        private readonly PlaneaOpciones _opciones = opciones.Value;

        public async Task<ResultadoSincronizacionPlanea> SincronizarAsync(PeriodoPorSincronizar periodo, CancellationToken cancellationToken = default)
        {
            var idBitacora = await repositorio.IniciarBitacoraAsync(periodo.IdPeriodo, cancellationToken);
            var reloj = Stopwatch.StartNew();
            DatosPeriodoPlanea? datos = null;
            int? recibidos = null;
            try
            {
                var respuesta = await ObtenerConReintentosAsync(periodo.Codigo, cancellationToken);
                recibidos = respuesta.Resultado?.Count ?? 0;
                if (respuesta.Periodo?.Trim() != periodo.Codigo)
                    throw new PlaneaRespuestaInvalidaException($"PLANEA devolvió el periodo '{respuesta.Periodo?.Trim() ?? "(vacío)"}' y se esperaba '{periodo.Codigo}'.");
                if (respuesta.Total != recibidos)
                    throw new PlaneaRespuestaInvalidaException($"PLANEA indicó total={respuesta.Total}, pero resultado contiene {recibidos} filas.");
                if (respuesta.Total == 0)
                {
                    await repositorio.CerrarBitacoraAsync(idBitacora, PlaneaConstantes.ESTADO_SIN_DATOS, recibidos, null, null, null, null, cancellationToken);
                    return new(periodo.Codigo, PlaneaConstantes.ESTADO_SIN_DATOS, null, null);
                }
                datos = PlaneaNormalizador.Normalizar(respuesta);
                var resumen = await repositorio.RegistrarNuevasAsync(periodo.IdPeriodo, periodo.Codigo, idBitacora, datos, cancellationToken);
                var advertencias = string.Join(Environment.NewLine, datos.Advertencias);
                await repositorio.CerrarBitacoraAsync(idBitacora, PlaneaConstantes.ESTADO_EXITOSA, recibidos, datos, resumen,
                    string.IsNullOrEmpty(advertencias) ? null : advertencias, null, cancellationToken);
                reloj.Stop();
                logger.LogInformation("Sincronización PLANEA {Periodo}: {Duracion} ms, nuevas={Nuevas}, existentes={Existentes}, sin plan={SinPlan}, sin EE={SinExperiencia}, horarios={Horarios}",
                    periodo.Codigo, reloj.ElapsedMilliseconds, resumen.NrcNuevos, resumen.NrcExistentes, datos.NrcSinPlan, resumen.NrcSinExperiencia, resumen.HorariosInsertados);
                return new(periodo.Codigo, PlaneaConstantes.ESTADO_EXITOSA, resumen, null);
            }
            catch (SincronizacionEnCursoException ex)
            {
                await repositorio.CerrarBitacoraAsync(idBitacora, PlaneaConstantes.ESTADO_OMITIDA, recibidos, datos, null, null, ex.Message, cancellationToken);
                logger.LogWarning("Sincronización omitida para {Periodo}: {Mensaje}", periodo.Codigo, ex.Message);
                return new(periodo.Codigo, PlaneaConstantes.ESTADO_OMITIDA, null, ex.Message);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                await repositorio.CerrarBitacoraAsync(idBitacora, PlaneaConstantes.ESTADO_INTERRUMPIDA, recibidos, datos, null, null, "La ejecución fue cancelada.", CancellationToken.None);
                throw;
            }
            catch (Exception ex)
            {
                await repositorio.CerrarBitacoraAsync(idBitacora, PlaneaConstantes.ESTADO_FALLIDA, recibidos, datos, null,
                    datos is null ? null : string.Join(Environment.NewLine, datos.Advertencias), ex.Message, CancellationToken.None);
                logger.LogError(ex, "Falló la sincronización PLANEA del periodo {Periodo}.", periodo.Codigo);
                return new(periodo.Codigo, PlaneaConstantes.ESTADO_FALLIDA, null, ex.Message);
            }
        }

        private async Task<PlaneaRespuesta> ObtenerConReintentosAsync(string codigoPeriodo, CancellationToken ct)
        {
            for (var intento = 1; ; intento++)
            {
                try { return await cliente.ObtenerPeriodoAsync(codigoPeriodo, ct); }
                catch (Exception ex) when ((ex is HttpRequestException || ex is TaskCanceledException && !ct.IsCancellationRequested) && intento < _opciones.Intentos)
                {
                    var espera = TimeSpan.FromTicks((long)(_opciones.EsperaEntreIntentos.Ticks * Math.Pow(4, intento - 1)));
                    await Task.Delay(espera, ct);
                }
            }
        }
    }
}
