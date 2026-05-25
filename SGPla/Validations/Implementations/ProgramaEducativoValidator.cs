using SGPla.Mappers;
using SGPla.Models;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Models.InterfacesDTOs;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Interfaces;
using System.Text.RegularExpressions;

namespace SGPla.Validations.Implementations
{
    public class ProgramaEducativoValidator : IProgramaEducativoValidator
    {
        private readonly IProgramaEducativoRepository _programaEducativoRepository;
        private readonly IEntidadAcademicaRepository _entidadAcademicaRepository;
        private readonly IPlanEstudiosRepository _planEstudiosRepository;

        public ProgramaEducativoValidator(IProgramaEducativoRepository programaEducativoRepository, IEntidadAcademicaRepository entidadAcademicaRepository, IPlanEstudiosRepository planEstudiosRepository)
        {
            _programaEducativoRepository = programaEducativoRepository;
            _entidadAcademicaRepository = entidadAcademicaRepository;
            _planEstudiosRepository = planEstudiosRepository;
        }

        public async Task ValidarBusquedaPorFiltroAsync(BuscarProgramaEducativoDTO buscarProgramaEducativoDTO)
        {
            
            if (buscarProgramaEducativoDTO.Nombre?.Length > 100)
                throw new ArgumentException("El nombre no debe ser superior a 100 caracteres.");


            if (buscarProgramaEducativoDTO.IdAreaAcademica < 0)
                throw new ArgumentException("El área académica debe ser un número positivo.");

        }

        public async Task ValidarCreacionAsync(CrearProgramaEducativoDTO crearProgramaEducativoDTO)
        {
            ValidarCampos(crearProgramaEducativoDTO);
            await ValidarExistenciaEntidadAcademicaAsync(crearProgramaEducativoDTO.IdEntidadAcademica);
            await ValidarNoRepetidoAsync(crearProgramaEducativoDTO);
        }

        public async Task ValidarEdicionAsync(EditarProgramaEducativoDTO editarProgramaEducativoDTO)
        {
            ValidarCampos(editarProgramaEducativoDTO);
            await ValidarExistencia(editarProgramaEducativoDTO.IdProgramaEducativo);
            await ValidarExistenciaEntidadAcademicaAsync(editarProgramaEducativoDTO.IdEntidadAcademica);
            await ValidarNoRepetidoAsync(editarProgramaEducativoDTO);
        }

        private async Task<bool> ValidarExistencia(int idProgramaEducativo)
        {
            var existe = await _programaEducativoRepository.ObtenerPorIdAsync(idProgramaEducativo);
            if (existe is null)
                throw new ArgumentException("El programa educativo no existe.");
            return true;
        }

        private async Task ValidarExistenciaEntidadAcademicaAsync(int idEntidadAcademica)
        {
            var existe = await _entidadAcademicaRepository.ObtenerPorIdAsync(idEntidadAcademica);
            if (existe is null)
                throw new ArgumentException("La entidad académica no existe.");
        }

        private async Task ValidarNoRepetidoAsync(IProgramaEducativoDTO programaEducativoDTO)
        {
            var programaEducativo = new ProgramaEducativo
            {
                IdProgramaEducativo = programaEducativoDTO is EditarProgramaEducativoDTO editarProgramaEducativoDTO ? editarProgramaEducativoDTO.IdProgramaEducativo : 0,
                Nombre = programaEducativoDTO.Nombre,
                IdEntidadAcademica = programaEducativoDTO.IdEntidadAcademica
            };

            var existe = await _programaEducativoRepository.ExisteAsync(programaEducativo);


            if (programaEducativoDTO is CrearProgramaEducativoDTO crearDTO)
            {

                if (existe is not null)
                    throw new ArgumentException("Ya existe un programa educativo la misma clave.");
            }
            else if (programaEducativoDTO is EditarProgramaEducativoDTO editarDTO)
            {

                if (existe is not null && existe.IdProgramaEducativo != editarDTO.IdProgramaEducativo)
                    throw new ArgumentException("Ya existe un programa educativo la misma clave.");
            }
        }

        private static void ValidarCampos(IProgramaEducativoDTO programaEducativoDTO)
        {

            if (string.IsNullOrEmpty(programaEducativoDTO.Nombre))
                throw new ArgumentException("El nombre del programa educativo es obligatorio.");

            if (programaEducativoDTO.Nombre.Length > 100)
                throw new ArgumentException("El nombre no debe ser superior a 100 caracteres.");

            if (programaEducativoDTO.Campus.Length > 100)
                throw new ArgumentException("El campus no debe ser superior a 100 caracteres.");

            if (programaEducativoDTO.IdEntidadAcademica <= 0)
                throw new ArgumentException("La entidad académica es obligatoria.");
            if (programaEducativoDTO is EditarProgramaEducativoDTO editarDTO && editarDTO.IdProgramaEducativo <= 0)
                throw new ArgumentException("El ID del programa educativo no es válido.");
        }


        public async Task ValidarEliminarAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID del programa educativo no es válido.");

            var existe = await _programaEducativoRepository.ObtenerPorIdAsync(id);

            if (existe is null)
                throw new ArgumentException("El programa educativo no existe.");

            if (await _programaEducativoRepository.EstaAsociadoAPlan(id))
            {
                throw new ArgumentException("No se puede eliminar el programa educativo porque está asociado a un plan de estudio.");
            }
        }

        public async Task ValidarObtenerPorIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID del programa educativo no es válido.");
        }
    }
}
