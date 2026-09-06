namespace SGPla.Modules.PeriodosEscolares.Application.ActualizarPeriodoEscolar.Contracts;

public sealed record ActualizarPeriodoEscolarCommand(
    int IdPeriodoEscolar,
    int? Anio,
    string? Periodo);
