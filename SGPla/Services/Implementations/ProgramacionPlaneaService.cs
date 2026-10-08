using Microsoft.Extensions.Options;
using SGPla.Commons;
using SGPla.Mappers;
using SGPla.Models;
using SGPla.Models.DTOs.Planea;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations
{
    public class ProgramacionPlaneaService : IProgramacionPlaneaService
    {
        private const int LimiteMaximo = 500;

        private readonly IProgramacionPlaneaRepository _repositorio;
        private readonly ISincronizarPeriodoPlaneaService _sincronizarPeriodo;
        private readonly ISincronizarPeriodosVigentesService _sincronizarVigentes;
        private readonly PlaneaOpciones _opciones;

        public ProgramacionPlaneaService(
            IProgramacionPlaneaRepository repositorio,
            ISincronizarPeriodoPlaneaService sincronizarPeriodo,
            ISincronizarPeriodosVigentesService sincronizarVigentes,
            IOptions<PlaneaOpciones> opciones)
        {
            _repositorio = repositorio;
            _sincronizarPeriodo = sincronizarPeriodo;
            _sincronizarVigentes = sincronizarVigentes;
            _opciones = opciones.Value;
        }

        public async Task<ProgramacionPlaneaDTO> ObtenerAsync(FiltroProgramacionPlaneaDTO filtro, CancellationToken cancellationToken = default)
        {
            filtro.Busqueda = string.IsNullOrWhiteSpace(filtro.Busqueda) ? null : filtro.Busqueda.Trim();
            if (filtro.Busqueda?.Length > 100) filtro.Busqueda = filtro.Busqueda[..100];
            if (filtro.Limite is < 1 or > LimiteMaximo) filtro.Limite = 10;
            if (filtro.Pagina < 1) filtro.Pagina = 1;

            var ultima = await _repositorio.ObtenerUltimaSincronizacionAsync(filtro.IdPeriodo, cancellationToken);
            var (copias, total, pagina) = await _repositorio.ObtenerCopiasAsync(filtro, cancellationToken);
            return new ProgramacionPlaneaDTO(ultima, copias, total, pagina);
        }

        public async Task<EncabezadoProgramacionPlaneaDTO?> ObtenerEncabezadoAsync(
            int idPlanEstudios, int idPeriodo, CancellationToken cancellationToken = default)
        {
            var encabezado = await _repositorio.ObtenerEncabezadoAsync(idPlanEstudios, idPeriodo, cancellationToken);
            if (encabezado is null) return null;

            var periodo = PeriodoEscolarMapper.ToDTO(new Periodo { IdPeriodo = idPeriodo, Codigo = encabezado.CodigoPeriodo });
            return encabezado with { PeriodoMostrar = periodo.PeriodoMostrar };
        }

        public async Task<ProgramacionParaAprobarPlaneaDTO> ObtenerParaAprobarAsync(
            int idPlanEstudios, int idPeriodo, string? busqueda, CancellationToken cancellationToken = default)
        {
            busqueda = string.IsNullOrWhiteSpace(busqueda) ? null : busqueda.Trim();
            if (busqueda?.Length > 100) busqueda = busqueda[..100];

            var copias = await _repositorio.ObtenerCopiasParaAprobarAsync(idPlanEstudios, idPeriodo, busqueda, cancellationToken);
            var resumen = await _repositorio.ObtenerResumenAprobacionAsync(idPlanEstudios, idPeriodo, cancellationToken);
            var ultima = await _repositorio.ObtenerUltimaSincronizacionAsync(idPeriodo, cancellationToken);
            return new ProgramacionParaAprobarPlaneaDTO(ultima, copias, resumen);
        }

        public async Task<(bool Exito, string Mensaje, ResultadoAprobacionPlaneaDTO? Resultado)> AprobarAsync(
            int idPlanEstudios, int idPeriodo, IReadOnlyCollection<int> idsAprobados, string revisadoPor,
            CancellationToken cancellationToken = default)
        {
            var resumen = await _repositorio.ObtenerResumenAprobacionAsync(idPlanEstudios, idPeriodo, cancellationToken);
            if (resumen.Pendientes == 0)
                return (false, "No hay NRC pendientes por confirmar.", null);

            // Una lista vacía equivale a descartar todos los pendientes; el front pide confirmación explícita.
            var resultado = await _repositorio.AprobarAsync(
                idPlanEstudios, idPeriodo, idsAprobados, revisadoPor, cancellationToken);
            return (true, "Programación confirmada.", resultado);
        }

        public Task<bool> RestaurarAsync(int idExperienciaEducativaPeriodo, CancellationToken cancellationToken = default)
            => _repositorio.RestaurarAsync(idExperienciaEducativaPeriodo, cancellationToken);

        /// H/S/M de una oferta aprobada: suma de horas semanales de los horarios PLANEA, redondeada al entero más cercano.
        /// PLANEA guarda los bloques como 14:00–14:59, así que un bloque cuyo fin termina en :59 se cuenta completo (+1 min).
        /// Si no hay horarios (o suman 0) se usa el valor de la EE; si tampoco es numérico, 0.
        public static int CalcularHsm(IEnumerable<HorarioPlaneaDTO> horarios, string? horasExperiencia)
        {
            var minutos = horarios.Sum(h =>
            {
                var duracion = (h.HoraFin - h.HoraInicio).TotalMinutes;
                if (duracion <= 0) return 0d;
                return h.HoraFin.Minute == 59 ? duracion + 1 : duracion;
            });

            var horas = (int)Math.Round(minutos / 60d, MidpointRounding.AwayFromZero);
            if (horas > 0) return horas;

            return int.TryParse(horasExperiencia?.Trim(), out var horasEe) && horasEe > 0 ? horasEe : 0;
        }

        public async Task<(bool Exito, string Mensaje)> SincronizarAsync(int? idPeriodo, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_opciones.ApiKey))
                return (false, "No se puede sincronizar: falta configurar el token de PLANEA (Planea:ApiKey).");

            IReadOnlyList<ResultadoSincronizacionPlanea> resultados;
            if (idPeriodo.HasValue)
            {
                var periodo = await _repositorio.ObtenerPeriodoAsync(idPeriodo.Value, cancellationToken);
                if (periodo is null)
                    return (false, "El periodo seleccionado no existe.");
                resultados = [await _sincronizarPeriodo.SincronizarAsync(periodo, cancellationToken)];
            }
            else
            {
                resultados = await _sincronizarVigentes.EjecutarAsync(cancellationToken);
                if (resultados.Count == 0)
                    return (false, "No hay periodos vigentes para sincronizar. Selecciona un periodo.");
            }

            var exito = resultados.All(r => r.Estado is PlaneaConstantes.ESTADO_EXITOSA or PlaneaConstantes.ESTADO_SIN_DATOS);
            return (exito, "Sincronización PLANEA — " + string.Join(" | ", resultados.Select(Describir)));
        }

        private static string Describir(ResultadoSincronizacionPlanea r) => r.Estado switch
        {
            PlaneaConstantes.ESTADO_EXITOSA when r.Resumen is { } s =>
                $"{r.CodigoPeriodo}: {s.NrcNuevos} NRC nuevos, {s.NrcExistentes} ya registrados, "
                + $"{s.NrcSinExperiencia} sin EE en el catálogo de su región, {s.HorariosInsertados} horarios.",
            PlaneaConstantes.ESTADO_SIN_DATOS => $"{r.CodigoPeriodo}: PLANEA aún no tiene programación para este periodo.",
            _ => $"{r.CodigoPeriodo}: {r.Estado}{(string.IsNullOrWhiteSpace(r.Mensaje) ? "" : " — " + r.Mensaje)}"
        };
    }
}
