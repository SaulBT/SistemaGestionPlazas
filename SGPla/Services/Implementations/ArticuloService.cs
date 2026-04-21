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

        public async Task<DetallesArticuloDTO> EditarArticuloAsync(EditarArticuloDTO dto)
        {
            await _articuloValidator.ValidarEdicionAsync(dto);

            var articulo = await _articuloRepository.ActualizarArticuloAsync(ArticuloMapper.ToModel(dto));

            DetallesArticuloDTO resultado = ArticuloMapper.ToDTO(articulo);

            return resultado;
        }

        public  async Task<DetallesArticuloDTO> CrearArticuloAsync(CrearArticuloDTO dto)
        {
            
            await _articuloValidator.ValidarCreacionAsync(dto);

            var articulo = await _articuloRepository.CrearArticuloAsync(ArticuloMapper.ToModel(dto));
            
            DetallesArticuloDTO resultado = ArticuloMapper.ToDTO(articulo);

            return resultado;
        }

        public Task<bool> EliminarArticuloAsync(int id)
        {
            return _articuloRepository.EliminarArticuloAsync(id);
        }
        public async Task<DetallesArticuloDTO?> ObtenerArticuloPorIdAsync(int id)
        {
            await _articuloValidator.ValidarObtenerPorIdAsync(id);

            var articulo = await _articuloRepository.ObtenerArticuloPorIdAsync(id);

            if (articulo is null)
                return null;

            DetallesArticuloDTO resultado = ArticuloMapper.ToDTO(articulo);
            return resultado;
        }

        public async Task<IEnumerable<DetallesArticuloDTO>> ObtenerTodosAsync()
        {
            var articulos = await _articuloRepository.ObtenerTodosAsync();
            if (articulos is null)
                return Enumerable.Empty<DetallesArticuloDTO>();

            IEnumerable<DetallesArticuloDTO> resultado = articulos.Select(a => ArticuloMapper.ToDTO(a));

            return resultado;

        }

        public async Task<IEnumerable<DetallesArticuloDTO>> BuscarPorTerminoAsync(string busqueda)
        {
            await _articuloValidator.ValidarBusquedaPorTerminoAsync(busqueda);


            var articulos = await _articuloRepository.BuscarPorTerminoAsync(busqueda);
            if (articulos is null)
                return Enumerable.Empty<DetallesArticuloDTO>();

            IEnumerable<DetallesArticuloDTO> resultado = articulos.Select(a => ArticuloMapper.ToDTO(a));

            return resultado;
        }
    }
}
