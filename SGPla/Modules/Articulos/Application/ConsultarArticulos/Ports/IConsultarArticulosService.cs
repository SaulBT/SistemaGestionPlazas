using SGPla.Modules.Articulos.Application.Contracts;
using SGPla.Modules.Articulos.Application.ConsultarArticulos.Contracts;

namespace SGPla.Modules.Articulos.Application.ConsultarArticulos;

public interface IConsultarArticulosService
{
    Task<IReadOnlyList<ArticuloResponse>> ConsultarAsync(
        ConsultarArticulosQuery query,
        CancellationToken cancellationToken);
}
