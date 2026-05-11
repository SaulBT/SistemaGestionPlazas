using SGPla.Mappers;
using SGPla.Models;
using SGPla.Models.DTOs.PeriodoEscolar;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Services.Implementations
{
    public class PeriodoEscolarService : IPeriodoEscolarService
    {
        private readonly IPeriodoEscolarRepository _periodoEscolarRepository;
        private readonly IPeriodoEscolarValidator _periodoEscolarValidator;

        public PeriodoEscolarService(IPeriodoEscolarRepository periodoEscolarRepository, IPeriodoEscolarValidator periodoEscolarValidator)
        {
            _periodoEscolarRepository = periodoEscolarRepository;
            _periodoEscolarValidator = periodoEscolarValidator;
        }

        public async Task<List<DetallesPeriodoEscolarDTO>> BuscarPorFiltroAsync(BuscarPeriodoEscolarDTO filtro)
        {
            await _periodoEscolarValidator.ValidarBusquedaPorFiltroAsync(filtro);

            var obtenidos = await _periodoEscolarRepository.ObtenerPorFiltroAsync(filtro)
                            ?? new List<Periodo>();

            return obtenidos
                .Where(x => x != null)
                .Select(PeriodoEscolarMapper.ToDTO)
                .ToList();
        }

        // Nuevo método que devuelve los items paginados con el total
        public async Task<(List<DetallesPeriodoEscolarDTO> Items, int TotalCount)> BuscarPorFiltroPaginadoAsync(BuscarPeriodoEscolarDTO filtro)
        {
            await _periodoEscolarValidator.ValidarBusquedaPorFiltroAsync(filtro);

            // Ejecutar las operaciones de forma secuencial para evitar conflictos con DbContext
            var totalCount = await _periodoEscolarRepository.ContarPorFiltroAsync(filtro);
            var obtenidos = await _periodoEscolarRepository.ObtenerPorFiltroAsync(filtro) ?? new List<Periodo>();

            var dtos = obtenidos
                .Where(x => x != null)
                .Select(PeriodoEscolarMapper.ToDTO)
                .ToList();

            return (dtos, totalCount);
        }


        public async Task<DetallesPeriodoEscolarDTO> CrearAsync(CrearPeriodoEscolarDTO periodoEscolarDTO)
        {
            await _periodoEscolarValidator.ValidarCreacionAsync(periodoEscolarDTO);
            var creado = await _periodoEscolarRepository.CrearAsync(PeriodoEscolarMapper.ToModel(periodoEscolarDTO));

            DetallesPeriodoEscolarDTO resultado = PeriodoEscolarMapper.ToDTO(creado);
            return resultado;
        }

        public async Task<DetallesPeriodoEscolarDTO> EditarAsync(EditarPeriodoEscolarDTO periodoEscolarDTO)
        {
            await _periodoEscolarValidator.ValidarEdicionAsync(periodoEscolarDTO);
            var editado = await _periodoEscolarRepository.ActualizarAsync(PeriodoEscolarMapper.ToModel(periodoEscolarDTO));

            DetallesPeriodoEscolarDTO resultado = PeriodoEscolarMapper.ToDTO(editado);
            return resultado;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            await _periodoEscolarValidator.ValidarEliminarAsync(id);
            return await _periodoEscolarRepository.EliminarAsync(id);
        }

        public async Task<DetallesPeriodoEscolarDTO?> ObtenerPorIdAsync(int id)
        {
            await _periodoEscolarValidator.ValidarObtenerPorIdAsync(id);
            var obtenido = await _periodoEscolarRepository.ObtenerPorIdAsync(id);
            DetallesPeriodoEscolarDTO resultado = PeriodoEscolarMapper.ToDTO(obtenido);
            return resultado;
        }

        public async Task<List<DetallesPeriodoEscolarDTO>> ObtenerTodosAsync()
        {
            var obtenidos = await _periodoEscolarRepository.ObtenerTodosAsync();
            List<DetallesPeriodoEscolarDTO> resultados = obtenidos.Select(PeriodoEscolarMapper.ToDTO).ToList();
            return resultados;
        }
    }
}
