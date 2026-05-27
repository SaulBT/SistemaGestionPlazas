using SGPla.Mappers;
using SGPla.Models;
using SGPla.Models.DTOs.PeriodoEscolar;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Models.InterfacesDTOs;
using SGPla.Repositories.Implementations;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Interfaces;
using System.Text.RegularExpressions;
using static SGPla.Mappers.PeriodoEscolarMapper;

namespace SGPla.Validations.Implementations
{
    public class PeriodoEscolarValidator : IPeriodoEscolarValidator
    {
        private readonly IPeriodoEscolarRepository _periodoEscolarRepository;

        public PeriodoEscolarValidator(IPeriodoEscolarRepository periodoEscolarRepository)
        {
            _periodoEscolarRepository = periodoEscolarRepository;
        }
        public Task ValidarBusquedaPorFiltroAsync(BuscarPeriodoEscolarDTO buscarPeriodoEscolarDTO)
        {
            if (buscarPeriodoEscolarDTO.Periodo != null)
            {
                buscarPeriodoEscolarDTO.PeriodoCodigo = PeriodoMapper.NombreACodigo.TryGetValue(buscarPeriodoEscolarDTO.Periodo, out var periodoCodigo) ? periodoCodigo : null;
            }

            if (buscarPeriodoEscolarDTO.Anio.HasValue &&
                   (buscarPeriodoEscolarDTO.Anio < 2000 || buscarPeriodoEscolarDTO.Anio > 2100))
            {
                throw new ArgumentException("Ingrese un año válido.");
            }
            return Task.CompletedTask;
        }

        public async Task ValidarCreacionAsync(CrearPeriodoEscolarDTO crearPeriodoEscolarDTO)
        {
            ValidarCampos(crearPeriodoEscolarDTO);
            await ValidarNoRepeticionAsync(crearPeriodoEscolarDTO);
        }

        public async Task ValidarEdicionAsync(EditarPeriodoEscolarDTO editarPeriodoEscolarDTO)
        {
            ValidarCampos(editarPeriodoEscolarDTO);
            await ValidarExistenciaAsync(editarPeriodoEscolarDTO.IdPeriodoEscolar);
            await ValidarNoRepeticionAsync(editarPeriodoEscolarDTO);
        }

        private static void ValidarCampos(IPeriodoEscolarDTO periodoEscolarDTO)
        {
            if (periodoEscolarDTO.Anio <= 0)
            {
                throw new ArgumentException("El año debe ser un número positivo.");
            }

            if (string.IsNullOrWhiteSpace(periodoEscolarDTO.Periodo))
            {
                throw new ArgumentException("El periodo no puede estar vacío.");
            }

            if (periodoEscolarDTO.Periodo != "Febrero-Julio" && periodoEscolarDTO.Periodo != "Agosto-Enero")
            {
                throw new ArgumentException("El periodo no es un periodo válido.");
            }

        }

        private async Task ValidarNoRepeticionAsync(IPeriodoEscolarDTO periodoEscolarDTO)
        {
            PeriodoMapper.NombreACodigo.TryGetValue(periodoEscolarDTO.Periodo, out var periodoCodigo);

            var periodo = new Periodo
            {
                IdPeriodo = periodoEscolarDTO is EditarPeriodoEscolarDTO editarPeriodoDTO ? editarPeriodoDTO.IdPeriodoEscolar : 0,
                Codigo = periodoEscolarDTO.Anio + periodoCodigo
            };

            var existe = await _periodoEscolarRepository.ExisteAsync(periodo);
            if (periodoEscolarDTO is CrearPeriodoEscolarDTO crearDTO)
            {

                if (existe is not null)
                    throw new ArgumentException("Ya existe un periodo escolar con los mismos datos.");
            }
            else if (periodoEscolarDTO is EditarPeriodoEscolarDTO editarDTO)
            {

                if (existe is not null && existe.IdPeriodo != editarDTO.IdPeriodoEscolar)
                    throw new ArgumentException("Ya existe un periodo escolar con los mismos datos.");
            }
        }

        private async Task<bool> ValidarExistenciaAsync(int idPeriodoEscolar)
        {
            var existe = await _periodoEscolarRepository.ObtenerPorIdAsync(idPeriodoEscolar);
            if (existe is null)
                throw new ArgumentException("El periodo escolar no existe.");
            return true;
        }

        public async Task ValidarEliminarAsync(int id)
        {
            if (await _periodoEscolarRepository.TieneRelacionesAsync(id))
            {
                throw new ArgumentException("El periodo escolar ya está relacionado a un aviso u oferta.");

            }

            if (id <= 0)
                throw new ArgumentException("El ID del periodo escolar no es válido.");

            var existe = await _periodoEscolarRepository.ObtenerPorIdAsync(id);

            if (existe is null)
                throw new ArgumentException("El periodo escolar no existe.");
        }

        public async Task ValidarObtenerPorIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID del periodo escolar no es válido.");
        }
    }
}
