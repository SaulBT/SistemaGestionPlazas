using SGPla.Modules.Articulos.Application.Contracts;
using SGPla.Modules.Articulos.Application.CrearArticulo.Contracts;

namespace SGPla.Modules.Articulos.Application.CrearArticulo;

public interface ICrearArticuloService
{
    Task<ArticuloResultado<ArticuloResponse>> CrearAsync(
        CrearArticuloCommand command,
        CancellationToken cancellationToken);
}
