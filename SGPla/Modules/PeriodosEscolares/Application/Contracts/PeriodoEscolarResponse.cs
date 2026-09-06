namespace SGPla.Modules.PeriodosEscolares.Application.Contracts;

public sealed record PeriodoEscolarResponse(
    int IdPeriodoEscolar,
    string Codigo,
    string Periodo,
    int Anio,
    string PeriodoMostrar);

public sealed record PeriodosEscolaresResponse(
    IReadOnlyList<PeriodoEscolarResponse> Items,
    int Pagina,
    int Cantidad,
    int Total);
