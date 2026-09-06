using SGPla.Modules.PeriodosEscolares.Application.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodosEscolares.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodosEscolares.Ports;
using SGPla.Modules.PeriodosEscolares.Application.Models;
using SGPla.Modules.PeriodosEscolares.Application.Ports;
using SGPla.Modules.PeriodosEscolares.Domain;

namespace SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodosEscolares;

public sealed class ConsultarPeriodosEscolaresService : IConsultarPeriodosEscolaresService
{
    private readonly IPeriodoEscolarRepository _repository;

    public ConsultarPeriodosEscolaresService(IPeriodoEscolarRepository repository)
    {
        _repository = repository;
    }

    public async Task<PeriodoEscolarResultado<PeriodosEscolaresResponse>> ConsultarAsync(
        ConsultarPeriodosEscolaresQuery query,
        CancellationToken cancellationToken)
    {
        var validacion = PeriodoEscolarReglas.ValidarConsulta(
            query.Anio,
            query.Periodo,
            query.Pagina,
            query.Cantidad);

        if (validacion is not null)
        {
            return PeriodoEscolarResultado<PeriodosEscolaresResponse>.Error(
                TipoResultadoPeriodoEscolar.Validacion,
                validacion.Mensaje,
                validacion.Campo);
        }

        string? codigoPeriodo = null;
        if (!string.IsNullOrWhiteSpace(query.Periodo))
        {
            PeriodoEscolarReglas.TryObtenerCodigoPeriodo(
                query.Periodo,
                out codigoPeriodo);
        }

        var pagina = await _repository.ObtenerPorFiltroAsync(
            new PeriodoEscolarFiltro(
                query.Anio,
                codigoPeriodo,
                query.Pagina,
                query.Cantidad),
            cancellationToken);

        var items = pagina.Items
            .Select(Mapear)
            .ToList();

        return PeriodoEscolarResultado<PeriodosEscolaresResponse>.Exito(
            new PeriodosEscolaresResponse(
                items,
                query.Pagina,
                query.Cantidad,
                pagina.Total));
    }

    private static PeriodoEscolarResponse Mapear(PeriodoEscolarRegistro registro)
    {
        var detalle = PeriodoEscolarReglas.CrearDetalle(
            registro.IdPeriodoEscolar,
            registro.Codigo);

        return new PeriodoEscolarResponse(
            detalle.IdPeriodoEscolar,
            detalle.Codigo,
            detalle.Periodo,
            detalle.Anio,
            detalle.PeriodoMostrar);
    }
}
