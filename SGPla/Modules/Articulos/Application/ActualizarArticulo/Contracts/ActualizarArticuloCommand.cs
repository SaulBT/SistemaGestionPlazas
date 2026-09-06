namespace SGPla.Modules.Articulos.Application.ActualizarArticulo.Contracts;

public sealed record ActualizarArticuloCommand(
    int IdArticulo,
    string? Numero,
    string? Descripcion);
