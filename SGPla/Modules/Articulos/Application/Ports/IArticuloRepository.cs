using SGPla.Modules.Articulos.Application.Models;

namespace SGPla.Modules.Articulos.Application.Ports;

public interface IArticuloRepository
{
    Task<IReadOnlyList<ArticuloConsulta>> ObtenerTodosAsync(
        string? busqueda,
        CancellationToken cancellationToken);

    Task<ArticuloConsulta?> ObtenerPorIdAsync(
        int idArticulo,
        CancellationToken cancellationToken);

    Task<bool> ExistePorNumeroAsync(
        string numero,
        int? idArticuloExcluido,
        CancellationToken cancellationToken);

    Task<ArticuloConsulta> CrearAsync(
        ArticuloParaCrear articulo,
        CancellationToken cancellationToken);

    Task<ArticuloConsulta?> ActualizarAsync(
        ArticuloParaActualizar articulo,
        CancellationToken cancellationToken);

    Task<bool> EliminarAsync(
        int idArticulo,
        CancellationToken cancellationToken);
}
