namespace SGPla.Models.DTOs.IntegranteCt;

public sealed record IntegranteConsejoTecnicoMvcFiltro(string? Busqueda, int Pagina = 1, int TamanoPagina = 20);

public sealed record IntegranteConsejoTecnicoMvcTratamiento(int Id, string Nombre, string Grado);

public sealed record IntegranteConsejoTecnicoMvcItem(int Id, string Nombre, string Cargo,
    int TratamientoAcademicoId, string Tratamiento, string Grado, DateOnly FechaInicio,
    DateOnly? FechaFin, bool TieneAsistencias);

public sealed record IntegranteConsejoTecnicoMvcPagina(IReadOnlyList<IntegranteConsejoTecnicoMvcItem> Items,
    int Total, int Pagina, int TamanoPagina, IReadOnlyList<IntegranteConsejoTecnicoMvcTratamiento> Tratamientos);

public sealed record IntegranteConsejoTecnicoMvcCambio(string Nombre, string Cargo,
    int TratamientoAcademicoId, DateOnly FechaInicio, DateOnly? FechaFin);
