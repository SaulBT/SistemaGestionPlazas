namespace SGPla.Models.DTOs.Docentes;

public sealed record DocenteDirectorioMvcFiltro(string? Busqueda = null, int Pagina = 1, int TamanoPagina = 25);
public sealed record DocenteDirectorioMvcFila(int Id, string Nombre, string? NumeroPersonal);
public sealed record PaginaDocenteDirectorioMvc(IReadOnlyList<DocenteDirectorioMvcFila> Items, int Total);
