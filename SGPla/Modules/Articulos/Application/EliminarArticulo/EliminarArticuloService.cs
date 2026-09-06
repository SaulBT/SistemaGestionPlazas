using SGPla.Modules.Articulos.Application.Contracts;
using SGPla.Modules.Articulos.Application.EliminarArticulo.Contracts;
using SGPla.Modules.Articulos.Application.Ports;

namespace SGPla.Modules.Articulos.Application.EliminarArticulo;

public sealed class EliminarArticuloService : IEliminarArticuloService
{
    private readonly IArticuloRepository _repository;

    public EliminarArticuloService(IArticuloRepository repository)
    {
        _repository = repository;
    }

    public async Task<ArticuloResultado<object?>> EliminarAsync(
        EliminarArticuloCommand command,
        CancellationToken cancellationToken)
    {
        if (command.IdArticulo <= 0)
        {
            return ArticuloResultado<object?>.Error(
                TipoResultadoArticulo.Validacion,
                "El ID del artículo no es válido.",
                "id");
        }

        var eliminado = await _repository.EliminarAsync(
            command.IdArticulo,
            cancellationToken);

        return eliminado
            ? new ArticuloResultado<object?>(TipoResultadoArticulo.Exito)
            : ArticuloResultado<object?>.Error(
                TipoResultadoArticulo.NoEncontrado,
                "El artículo indicado no existe.");
    }
}
