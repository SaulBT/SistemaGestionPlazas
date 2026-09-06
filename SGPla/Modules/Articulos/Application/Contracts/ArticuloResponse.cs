namespace SGPla.Modules.Articulos.Application.Contracts;

public sealed record ArticuloResponse(
    int IdArticulo,
    string Numero,
    string Descripcion);
