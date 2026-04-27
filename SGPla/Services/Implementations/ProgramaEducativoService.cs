using SGPla.Mappers;
using SGPla.Models;
using SGPla.Models.DTOs.AreaAcademica;
using SGPla.Models.DTOs.EntidadAcademica;
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
        private readonly IEntidadAcademicaRepository _entidadAcademicaRepository;
        private readonly IAreaAcademicaRepository _areaAcademicaRepository;     


        public ProgramaEducativoService(IProgramaEducativoRepository programaEducativoRepository, IProgramaEducativoValidator programaEducativoValidator, IEntidadAcademicaRepository entidadAcademicaRepository, IAreaAcademicaRepository areaAcademicaRepository)
        {
            _programaEducativoRepository = programaEducativoRepository;
            _programaEducativoValidator = programaEducativoValidator;
            _entidadAcademicaRepository = entidadAcademicaRepository;
            _areaAcademicaRepository = areaAcademicaRepository;
        }
        public async Task<List<DetallesProgramaEducativoDTO>> BuscarPorFiltroAsync(BuscarProgramaEducativoDTO filtro)
        {
            var obtenidos = await _programaEducativoRepository.ObtenerPorFiltroAsync(filtro)
                             ?? new List<ProgramaEducativo>();

            return obtenidos
                .Where(x => x != null)
                .Select(ProgramaEducativoMapper.ToDTO)
                .ToList();
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

        public async Task<List<OpcionAreaAcademicaDTO>> ObtenerOpcionesAreaAcademicaAsync()
        {
            await _areaAcademicaRepository.ObtenerTodosAsync();
            var obtenidos = await _areaAcademicaRepository.ObtenerTodosAsync();
            List<OpcionAreaAcademicaDTO> resultados = obtenidos.Select(AreaAcademicaMapper.ToOpcionDTO).ToList();
            return resultados;
        }

        public async Task<List<OpcionEntidadAcademicaDTO>> ObtenerOpcionesEntidadAcademicaAsync(string region, int idAreaAcademica)
        {
            var obtenidos = await _entidadAcademicaRepository.ObtenerOpcionesAsync(region, idAreaAcademica);
            List<OpcionEntidadAcademicaDTO> resultados = obtenidos.Select(EntidadAcademicaMapper.ToOpcionDTO).ToList();
            return resultados;
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
