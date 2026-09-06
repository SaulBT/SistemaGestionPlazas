namespace SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodosEscolares.Contracts;

public sealed record ConsultarPeriodosEscolaresQuery(
    int? Anio,
    string? Periodo,
    int Pagina,
    int Cantidad);
