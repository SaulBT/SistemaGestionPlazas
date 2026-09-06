using SGPla.Modules.Articulos.Application.Contracts;
using SGPla.Modules.Articulos.Application.ConsultarArticulos.Contracts;
using SGPla.Modules.Articulos.Application.Ports;

namespace SGPla.Modules.Articulos.Application.ConsultarArticulos;

public sealed class ConsultarArticulosService : IConsultarArticulosService
{
    private readonly IArticuloRepository _repository;

    public ConsultarArticulosService(IArticuloRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ArticuloResponse>> ConsultarAsync(
        ConsultarArticulosQuery query,
        CancellationToken cancellationToken)
    {
        var articulos = await _repository.ObtenerTodosAsync(
            query.Busqueda,
            cancellationToken);

        return articulos
            .Select(articulo => new ArticuloResponse(
                articulo.IdArticulo,
                articulo.Numero,
                articulo.Descripcion))
            .ToList();
    }
}
