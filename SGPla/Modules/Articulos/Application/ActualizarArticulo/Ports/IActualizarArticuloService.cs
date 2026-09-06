using SGPla.Modules.Articulos.Application.Contracts;
using SGPla.Modules.Articulos.Application.ActualizarArticulo.Contracts;

namespace SGPla.Modules.Articulos.Application.ActualizarArticulo;

public interface IActualizarArticuloService
{
    Task<ArticuloResultado<ArticuloResponse>> ActualizarAsync(
        ActualizarArticuloCommand command,
        CancellationToken cancellationToken);
}
