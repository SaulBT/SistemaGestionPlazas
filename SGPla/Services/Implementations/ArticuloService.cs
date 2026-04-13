using SGPla.Mappers;
using SGPla.Models;
using SGPla.Models.DTOs.Articulo;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Services.Implementations
{
    public class ArticuloService : IArticuloService
    {
        private readonly IArticuloRepository _articuloRepository;
        private readonly IArticuloValidator _articuloValidator;

        public ArticuloService(IArticuloRepository articuloRepository, IArticuloValidator articuloValidator)
        {
            _articuloRepository = articuloRepository;
            _articuloValidator = articuloValidator;
        }

        public Task ActualizarArticuloAsync(Articulo articulo)
        {
            return _articuloRepository.ActualizarArticuloAsync(articulo);
        }

        public  async Task<ArticuloDTO> CrearArticuloAsync(FormularioArticuloDTO dto)
        {
            
            
            await _articuloValidator.ValidarCreacionAsync(dto);
            var articulo = await _articuloRepository.CrearArticuloAsync(ArticuloMapper.ToModel(dto));
            
            ArticuloDTO response = ArticuloMapper.ToDTO(articulo);

            return response;
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
