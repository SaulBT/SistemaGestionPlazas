namespace SGPla.Modules.Articulos.Application.Models;

public sealed record ArticuloConsulta(
    int IdArticulo,
    string Numero,
    string Descripcion);

public sealed record ArticuloParaCrear(
    string Numero,
    string Descripcion);

public sealed record ArticuloParaActualizar(
    int IdArticulo,
    string Numero,
    string Descripcion);
