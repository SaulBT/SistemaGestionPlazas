namespace SGPla.Modules.PeriodosEscolares.Application.CrearPeriodoEscolar.Contracts;

public sealed record CrearPeriodoEscolarCommand(
    int? Anio,
    string? Periodo);
