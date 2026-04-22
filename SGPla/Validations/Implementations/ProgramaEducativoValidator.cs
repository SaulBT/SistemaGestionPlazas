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

        public ProgramaEducativoValidator(IProgramaEducativoRepository programaEducativoRepository)
        {
            _programaEducativoRepository = programaEducativoRepository;
        }

        public async Task ValidarBusquedaPorFiltroAsync(BuscarProgramaEducativoDTO buscarProgramaEducativoDTO)
        {
            ValidarCampos(buscarProgramaEducativoDTO);
        }

        public async Task ValidarCreacionAsync(CrearProgramaEducativoDTO crearProgramaEducativoDTO)
        {
            ValidarCampos(crearProgramaEducativoDTO);
            await ValidarNoRepetidoAsync(crearProgramaEducativoDTO);
        }

        public async Task ValidarEdicionAsync(EditarProgramaEducativoDTO editarProgramaEducativoDTO)
        {
            ValidarCampos(editarProgramaEducativoDTO);
            await ValidarNoRepetidoAsync(editarProgramaEducativoDTO);
        }

        private async Task ValidarNoRepetidoAsync(IProgramaEducativoDTO programaEducativoDTO)
        {
            var programaEducativo = new ProgramaEducativo
            {
                IdProgramaEducativo = programaEducativoDTO is EditarProgramaEducativoDTO editarProgramaEducativoDTO ? editarProgramaEducativoDTO.IdProgramaEducativo : 0,
                Nombre = programaEducativoDTO.Nombre,
                IdEntidadAcademica = programaEducativoDTO.IdEntidadAcademica
            };

            if (programaEducativoDTO is CrearProgramaEducativoDTO crearDTO)
            {
                var existe = await _programaEducativoRepository.ExisteAsync(programaEducativo);

                if (existe is not null)
                    throw new ArgumentException("Ya existe un programa educativo con el mismo nombre en la misma entidad académica.");
            }
            else if (programaEducativoDTO is EditarProgramaEducativoDTO editarDTO)
            {
                var existe = await _programaEducativoRepository.ExisteAsync(programaEducativo);

                if (existe is not null && existe.IdProgramaEducativo != editarDTO.IdProgramaEducativo)
                    throw new ArgumentException("Ya existe un programa educativo con el mismo nombre en la misma entidad académica.");
            }
        }

        private static void ValidarCampos(IProgramaEducativoDTO programaEducativoDTO)
        {
            if (programaEducativoDTO is not BuscarProgramaEducativoDTO && string.IsNullOrWhiteSpace(programaEducativoDTO.Nombre))
                throw new ArgumentException("El nombre del programa educativo es obligatorio.");

            if (programaEducativoDTO.Nombre.Length > 100)
                throw new ArgumentException("El nombre no debe ser superior a 100 caracteres.");

            if (!Regex.IsMatch(programaEducativoDTO.Nombre, @"^[a-zA-Z0-9\s]+$"))
                throw new ArgumentException("El nombre del programa educativo solo puede contener letras, números y espacios.");

            if (programaEducativoDTO.IdEntidadAcademica <= 0)
                throw new ArgumentException("La entidad académica es obligatoria.");

            if (programaEducativoDTO is BuscarProgramaEducativoDTO buscarDTO)
            {
                if (buscarDTO.IdRegion < 0)
                    throw new ArgumentException("La región debe ser un número positivo.");

                if (buscarDTO.IdAreaAcademica < 0)
                    throw new ArgumentException("El área académica debe ser un número positivo.");
            }
        }

        public async Task ValidarEliminarAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID del programa educativo no es válido.");

            var existe = await _programaEducativoRepository.ObtenerPorIdAsync(id);

            if (existe is null)
                throw new ArgumentException("El programa educativo no existe.");
        }

        public async Task ValidarObtenerPorIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID del programa educativo no es válido.");
        }
    }
}
