using SGPla.Models;
using SGPla.Repositories;

namespace SGPla.Services
{
    public class ArticuloService : IArticuloService
    {
        private readonly IArticuloRepository _articuloRepository;

        public ArticuloService(IArticuloRepository articuloRepository)
        {
            _articuloRepository = articuloRepository;
        }

        public Task ActualizarArticuloAsync(Articulo articulo)
        {
            return _articuloRepository.ActualizarArticuloAsync(articulo);
        }

        public  Task CrearArticuloAsync(Articulo articulo)
        {
            return _articuloRepository.CrearArticuloAsync(articulo);
        }

        public Task EliminarArticuloAsync(int id)
        {
            return _articuloRepository.EliminarArticuloAsync(id);
        }
        public async Task<Articulo?> ObtenerArticuloPorIdAsync(int id)
        {
            return await _articuloRepository.ObtenerArticuloPorIdAsync(id);
        }

        public async Task<IEnumerable<Articulo>> ObtenerTodosAsync()
        {
            return await _articuloRepository.ObtenerTodosAsync();
        }
    }
}
