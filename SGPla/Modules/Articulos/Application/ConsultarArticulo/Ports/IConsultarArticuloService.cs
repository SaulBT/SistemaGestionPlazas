using SGPla.Modules.Articulos.Application.Contracts;
using SGPla.Modules.Articulos.Application.ConsultarArticulo.Contracts;

namespace SGPla.Modules.Articulos.Application.ConsultarArticulo;

public interface IConsultarArticuloService
{
    Task<ArticuloResultado<ArticuloResponse>> ConsultarAsync(
        ConsultarArticuloQuery query,
        CancellationToken cancellationToken);
}
