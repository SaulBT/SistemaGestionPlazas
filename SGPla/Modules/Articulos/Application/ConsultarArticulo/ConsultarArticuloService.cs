using SGPla.Modules.Articulos.Application.Contracts;
using SGPla.Modules.Articulos.Application.ConsultarArticulo.Contracts;
using SGPla.Modules.Articulos.Application.Ports;

namespace SGPla.Modules.Articulos.Application.ConsultarArticulo;

public sealed class ConsultarArticuloService : IConsultarArticuloService
{
    private readonly IArticuloRepository _repository;

    public ConsultarArticuloService(IArticuloRepository repository)
    {
        _repository = repository;
    }

    public async Task<ArticuloResultado<ArticuloResponse>> ConsultarAsync(
        ConsultarArticuloQuery query,
        CancellationToken cancellationToken)
    {
        if (query.IdArticulo <= 0)
        {
            return ArticuloResultado<ArticuloResponse>.Error(
                TipoResultadoArticulo.Validacion,
                "El ID del artículo no es válido.",
                "id");
        }

        var articulo = await _repository.ObtenerPorIdAsync(
            query.IdArticulo,
            cancellationToken);

        return articulo is null
            ? ArticuloResultado<ArticuloResponse>.Error(
                TipoResultadoArticulo.NoEncontrado,
                "El artículo indicado no existe.")
            : ArticuloResultado<ArticuloResponse>.Exito(new ArticuloResponse(
                articulo.IdArticulo,
                articulo.Numero,
                articulo.Descripcion));
    }
}
