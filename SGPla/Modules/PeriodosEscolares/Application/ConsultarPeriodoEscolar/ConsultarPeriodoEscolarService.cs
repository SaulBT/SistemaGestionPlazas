using SGPla.Modules.PeriodosEscolares.Application.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodoEscolar.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodoEscolar.Ports;
using SGPla.Modules.PeriodosEscolares.Application.Ports;
using SGPla.Modules.PeriodosEscolares.Domain;

namespace SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodoEscolar;

public sealed class ConsultarPeriodoEscolarService : IConsultarPeriodoEscolarService
{
    private readonly IPeriodoEscolarRepository _repository;

    public ConsultarPeriodoEscolarService(IPeriodoEscolarRepository repository)
    {
        _repository = repository;
    }

    public async Task<PeriodoEscolarResultado<PeriodoEscolarResponse>> ConsultarAsync(
        ConsultarPeriodoEscolarQuery query,
        CancellationToken cancellationToken)
    {
        if (query.IdPeriodoEscolar <= 0)
        {
            return PeriodoEscolarResultado<PeriodoEscolarResponse>.Error(
                TipoResultadoPeriodoEscolar.Validacion,
                "El ID del periodo escolar no es válido.",
                "idPeriodoEscolar");
        }

        var registro = await _repository.ObtenerPorIdAsync(
            query.IdPeriodoEscolar,
            cancellationToken);

        if (registro is null)
        {
            return PeriodoEscolarResultado<PeriodoEscolarResponse>.Error(
                TipoResultadoPeriodoEscolar.NoEncontrado,
                "El periodo escolar indicado no existe.");
        }

        var detalle = PeriodoEscolarReglas.CrearDetalle(
            registro.IdPeriodoEscolar,
            registro.Codigo);

        return PeriodoEscolarResultado<PeriodoEscolarResponse>.Exito(
            new PeriodoEscolarResponse(
                detalle.IdPeriodoEscolar,
                detalle.Codigo,
                detalle.Periodo,
                detalle.Anio,
                detalle.PeriodoMostrar));
    }
}
