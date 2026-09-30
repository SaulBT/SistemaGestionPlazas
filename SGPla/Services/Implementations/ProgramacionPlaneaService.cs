using Microsoft.Extensions.Options;
using SGPla.Commons;
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
            if (filtro.Limite is < 1 or > LimiteMaximo) filtro.Limite = 100;

            var ultima = await _repositorio.ObtenerUltimaSincronizacionAsync(filtro.IdPeriodo, cancellationToken);
            var (copias, total) = await _repositorio.ObtenerCopiasAsync(filtro, cancellationToken);
            return new ProgramacionPlaneaDTO(ultima, copias, total);
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
