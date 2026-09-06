namespace SGPla.Modules.Articulos.Application.CrearArticulo.Contracts;

public sealed record CrearArticuloCommand(
    string? Numero,
    string? Descripcion);
