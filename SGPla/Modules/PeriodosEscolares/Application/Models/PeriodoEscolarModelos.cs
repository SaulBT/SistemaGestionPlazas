namespace SGPla.Modules.PeriodosEscolares.Application.Models;

public sealed record PeriodoEscolarRegistro(
    int IdPeriodoEscolar,
    string Codigo);

public sealed record PeriodoEscolarParaCrear(string Codigo);

public sealed record PeriodoEscolarParaActualizar(
    int IdPeriodoEscolar,
    string Codigo);

public sealed record PeriodoEscolarFiltro(
    int? Anio,
    string? CodigoPeriodo,
    int Pagina,
    int Cantidad);

public sealed record PeriodosEscolaresPagina(
    IReadOnlyList<PeriodoEscolarRegistro> Items,
    int Total);
