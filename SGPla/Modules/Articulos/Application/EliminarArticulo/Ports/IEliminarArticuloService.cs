using SGPla.Modules.Articulos.Application.Contracts;
using SGPla.Modules.Articulos.Application.EliminarArticulo.Contracts;

namespace SGPla.Modules.Articulos.Application.EliminarArticulo;

public interface IEliminarArticuloService
{
    Task<ArticuloResultado<object?>> EliminarAsync(
        EliminarArticuloCommand command,
        CancellationToken cancellationToken);
}
