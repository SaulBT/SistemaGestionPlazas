using SGPla.Modules.Articulos.Application.ActualizarArticulo.Contracts;
using SGPla.Modules.Articulos.Application.Contracts;
using SGPla.Modules.Articulos.Application.Models;
using SGPla.Modules.Articulos.Application.Ports;
using SGPla.Modules.Articulos.Domain;

namespace SGPla.Modules.Articulos.Application.ActualizarArticulo;

public sealed class ActualizarArticuloService : IActualizarArticuloService
{
    private readonly IArticuloRepository _repository;

    public ActualizarArticuloService(IArticuloRepository repository)
    {
        _repository = repository;
    }

    public async Task<ArticuloResultado<ArticuloResponse>> ActualizarAsync(
        ActualizarArticuloCommand command,
        CancellationToken cancellationToken)
    {
        if (command.IdArticulo <= 0)
        {
            return ArticuloResultado<ArticuloResponse>.Error(
                TipoResultadoArticulo.Validacion,
                "El ID del artículo no es válido.",
                "id");
        }

        var datos = ArticuloReglas.Normalizar(command.Numero, command.Descripcion);
        var validacion = ArticuloReglas.Validar(datos.Numero, datos.Descripcion);

        if (validacion is not null)
        {
            return ArticuloResultado<ArticuloResponse>.Error(
                TipoResultadoArticulo.Validacion,
                validacion.Mensaje,
                validacion.Campo);
        }

        var existente = await _repository.ObtenerPorIdAsync(
            command.IdArticulo,
            cancellationToken);

        if (existente is null)
        {
            return ArticuloResultado<ArticuloResponse>.Error(
                TipoResultadoArticulo.NoEncontrado,
                "El artículo indicado no existe.");
        }

        if (await _repository.ExistePorNumeroAsync(
                datos.Numero!,
                command.IdArticulo,
                cancellationToken))
        {
            return ArticuloResultado<ArticuloResponse>.Error(
                TipoResultadoArticulo.Conflicto,
                "Ya existe un artículo con el mismo número.");
        }

        var articulo = await _repository.ActualizarAsync(
            new ArticuloParaActualizar(
                command.IdArticulo,
                datos.Numero!,
                datos.Descripcion!),
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
