using SGPla.Mappers;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Services.Implementations
{
    public class ProgramaEducativoService : IProgramaEducativoService
    {
        private readonly IProgramaEducativoRepository _programaEducativoRepository;
        private readonly IProgramaEducativoValidator _programaEducativoValidator;

        public ProgramaEducativoService(IProgramaEducativoRepository programaEducativoRepository, IProgramaEducativoValidator programaEducativoValidator)
        {
            _programaEducativoRepository = programaEducativoRepository;
            _programaEducativoValidator = programaEducativoValidator;
        }
        public Task<List<DetallesProgramaEducativoDTO>> BuscarPorFiltroAsync(BuscarProgramaEducativoDTO filtro)
        {
            throw new NotImplementedException();
        }

        public async Task<DetallesProgramaEducativoDTO> CrearAsync(CrearProgramaEducativoDTO programaEducativo)
        {
            await _programaEducativoValidator.ValidarCreacionAsync(programaEducativo);
            var creado = await _programaEducativoRepository.CrearAsync(ProgramaEducativoMapper.ToModel(programaEducativo));

            DetallesProgramaEducativoDTO resultado = ProgramaEducativoMapper.ToDTO(creado);
            return resultado;
        }

        public async Task<DetallesProgramaEducativoDTO> EditarAsync(EditarProgramaEducativoDTO programaEducativo)
        {
            await _programaEducativoValidator.ValidarEdicionAsync(programaEducativo);
            var editado = await _programaEducativoRepository.ActualizarAsync(ProgramaEducativoMapper.ToModel(programaEducativo)); 

            DetallesProgramaEducativoDTO resultado = ProgramaEducativoMapper.ToDTO(editado);
            return resultado;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            await _programaEducativoValidator.ValidarEliminarAsync(id);
            return await _programaEducativoRepository.EliminarAsync(id);
        }

        public async Task<DetallesProgramaEducativoDTO?> ObtenerPorIdAsync(int id)
        {
            await _programaEducativoValidator.ValidarObtenerPorIdAsync(id);
            var obtenido = await _programaEducativoRepository.ObtenerPorIdAsync(id);
            DetallesProgramaEducativoDTO resultado = ProgramaEducativoMapper.ToDTO(obtenido);
            return resultado;
        }

        public async Task<List<DetallesProgramaEducativoDTO>> ObtenerTodosAsync()
        {
            var obtenidos = await _programaEducativoRepository.ObtenerTodosAsync();
            List<DetallesProgramaEducativoDTO> resultados = obtenidos.Select(ProgramaEducativoMapper.ToDTO).ToList();
            return resultados;
        }
    }
}
