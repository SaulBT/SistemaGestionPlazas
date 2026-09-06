using SGPla.Modules.Articulos.Application.Contracts;
using SGPla.Modules.Articulos.Application.CrearArticulo.Contracts;
using SGPla.Modules.Articulos.Application.Models;
using SGPla.Modules.Articulos.Application.Ports;
using SGPla.Modules.Articulos.Domain;

namespace SGPla.Modules.Articulos.Application.CrearArticulo;

public sealed class CrearArticuloService : ICrearArticuloService
{
    private readonly IArticuloRepository _repository;

    public CrearArticuloService(IArticuloRepository repository)
    {
        _repository = repository;
    }

    public async Task<ArticuloResultado<ArticuloResponse>> CrearAsync(
        CrearArticuloCommand command,
        CancellationToken cancellationToken)
    {
        var datos = ArticuloReglas.Normalizar(command.Numero, command.Descripcion);
        var validacion = ArticuloReglas.Validar(datos.Numero, datos.Descripcion);

        if (validacion is not null)
        {
            return ArticuloResultado<ArticuloResponse>.Error(
                TipoResultadoArticulo.Validacion,
                validacion.Mensaje,
                validacion.Campo);
        }

        if (await _repository.ExistePorNumeroAsync(
                datos.Numero!,
                null,
                cancellationToken))
        {
            return ArticuloResultado<ArticuloResponse>.Error(
                TipoResultadoArticulo.Conflicto,
                "Ya existe un artículo con el mismo número.");
        }

        var articulo = await _repository.CrearAsync(
            new ArticuloParaCrear(datos.Numero!, datos.Descripcion!),
            cancellationToken);

        return ArticuloResultado<ArticuloResponse>.Exito(new ArticuloResponse(
            articulo.IdArticulo,
            articulo.Numero,
            articulo.Descripcion));
    }
}
