using SGPla.Models.DTOs.Oferta;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Validations.Implementations
{
    public class ProgramacionAcademicaValidator : IProgramacionAcademicaValidator
    {
        private readonly IDocenteRepository _docenteRepository;
        private readonly IProgramaEducativoRepository _programaEducativoRepository;
        private readonly IProgramacionAcademicaRepository _programacionAcademicaRepository;

        public ProgramacionAcademicaValidator(IDocenteRepository docenteRepository, IProgramaEducativoRepository programaEducativoRepository, IProgramacionAcademicaRepository programacionAcademicaRepository  )
        {
            _docenteRepository = docenteRepository;
            _programaEducativoRepository = programaEducativoRepository;
            _programacionAcademicaRepository = programacionAcademicaRepository;
        }

        public async Task<bool> ValidarDocentes(List<OfertaDTO> ofertas)
        {
            var numerosPersonal = ofertas
    .Where(x => !string.IsNullOrWhiteSpace(x.NP))
    .Select(x => x.NP)
    .Distinct()
    .ToList();

            var registrados = await _docenteRepository
                .ObtenerNumerosPersonalRegistradosAsync(numerosPersonal);

            var noRegistrados = numerosPersonal
                .Except(registrados)
                .ToList();

            if (noRegistrados.Any())
            {
                throw new Exception(
                    $"Los siguientes docentes no están registrados dentro del sistema: " +
                    $"{string.Join(", ", noRegistrados)}");
            }

            return true;


        }

        public async Task<bool> ValidarProgramas(List<OfertaDTO> ofertas)
        {
            var programas = ofertas
                .Where(x => !string.IsNullOrWhiteSpace(x.Programa))
                .Select(x => x.Programa)
                .Distinct()
                .ToList();

            if (programas == null || programas.Count == 0)
            {
                throw new ArgumentException("No se han proporcionado programas educativos para validar.");
            }

            var registrados = await _programaEducativoRepository
                .ObtenerNombresProgramasRegistradosAsync(programas);
            var noRegistrados = programas
                .Except(registrados)
                .ToList();
            if (noRegistrados.Any())
            {
                throw new Exception(
                    $"Los siguientes programas educativos no están registrados dentro del sistema: " +
                    $"{string.Join(", ", noRegistrados)}");
            }
            return true;

        }

        public async Task<bool> ValidarExperiencias(List<OfertaDTO> ofertas)
        {
            var relaciones = ofertas
         .Where(x =>
             !string.IsNullOrWhiteSpace(x.Programa) &&
             !string.IsNullOrWhiteSpace(x.ExperienciaEducativa))
         .Select(x => $"{x.Programa}|{x.ExperienciaEducativa}")
         .Distinct()
         .ToList();

            var registradas = await _programacionAcademicaRepository
                .ObtenerRelacionesValidasAsync(ofertas);

            var noRegistradas = relaciones
                .Except(registradas)
                .ToList();

            if (noRegistradas.Any())
            {
                throw new Exception(
                    "Las siguientes experiencias no corresponden al programa educativo:\n" +
                    string.Join("\n", noRegistradas));
            }

            return true;

        }

    }
}
