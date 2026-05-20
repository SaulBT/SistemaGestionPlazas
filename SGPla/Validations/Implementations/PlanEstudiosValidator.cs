using System.Text.RegularExpressions;
using SGPla.Commons;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Validations.Implementations
{
    public class PlanEstudiosValidator : IPlanEstudiosValidator
    {

        private static readonly Regex regexCodigoExperienciaEducativa = new(
            "^[A-Z]{4} [0-9]{5}$",
            RegexOptions.Compiled);

        private readonly IPlanEstudiosRepository _planEstudiosRepository;
        private readonly IExperienciaEducativaRepository _experienciaEducativaRepository;
        private readonly ILogger<PlanEstudiosValidator> _logger;

        public PlanEstudiosValidator(
            IPlanEstudiosRepository planEstudiosRepository,
            IExperienciaEducativaRepository experienciaEducativaRepository,
            ILogger<PlanEstudiosValidator> logger)
        {
            _planEstudiosRepository = planEstudiosRepository;
            _experienciaEducativaRepository = experienciaEducativaRepository;
            _logger = logger;
        }

        public void ValidarArchivo(ArchivoPlanEstudiosDTO archivoPlanEstudiosDTO)
        {
            ArgumentNullException.ThrowIfNull(archivoPlanEstudiosDTO);

            if (string.IsNullOrEmpty(archivoPlanEstudiosDTO.Ruta))
                throw new ArgumentException("El Archivo es obligatorio.");
            if (string.IsNullOrWhiteSpace(archivoPlanEstudiosDTO.NombreArchivo))
                throw new ArgumentException("El NombreArchivo es obligatorio.");

            var extension = Path.GetExtension(archivoPlanEstudiosDTO.NombreArchivo).ToLowerInvariant();
            if (extension != ".xls" && extension != ".xlsx")
                throw new ArgumentException("El formato del archivo no es soportado.");
        }

        public void ValidarIndice(int indice)
        {
            if (indice <= 0)
            {
                _logger.LogError("PLAN ESTUDIOS: El Índice proporcionado es inválido: {Indice}", indice);
                throw new ArgumentException("El Índice es inválido.");
            }
        }

        public async Task ValidarCreacionAsync(CrearPlanEstudiosDTO crearPlanEstudiosDTO)
        {
            ArgumentNullException.ThrowIfNull(crearPlanEstudiosDTO);

            await validarIdProgramaEducativoAsync(crearPlanEstudiosDTO.IdProgramaEducativo);
            validarNombrePlanEstudios(crearPlanEstudiosDTO.Nombre);
            validarModalidad(crearPlanEstudiosDTO.Sistema);
            validarExperienciasEducativasParaCreacion(crearPlanEstudiosDTO.ExperienciasEducativas);

            foreach (var experienciaEducativa in crearPlanEstudiosDTO.ExperienciasEducativas)
            {
                validarCamposExperienciaEducativaNueva(experienciaEducativa);
                validarCodigoExperienciaEducativa(experienciaEducativa.Codigo, "El Código de una Experiencia Educativa es inválido.");

                bool existeEnSistema = await _experienciaEducativaRepository.ExisteCodigoExperienciaEducativaEnSistemaAsync(experienciaEducativa.Codigo.Trim());
                if (existeEnSistema)
                    throw new ArgumentException($"Ya hay una Experiencia Educativa con el Código {experienciaEducativa.Codigo.Trim()} en el sistema.");
            }
        }

        public async Task ValidarEdicionAsync(EditarPlanEstudiosDTO editarPlanEstudiosDTO)
        {
            ArgumentNullException.ThrowIfNull(editarPlanEstudiosDTO);

            await ValidarIdAsync(editarPlanEstudiosDTO.IdPlanEstudios);
            validarIdsExperienciasEliminadas(editarPlanEstudiosDTO.IdsExperienciasEliminadas);

            foreach (var idExperienciaEducativa in editarPlanEstudiosDTO.IdsExperienciasEliminadas)
            {
                bool existe = await _experienciaEducativaRepository.ExisteExperienciaEducativaPorIdAsync(idExperienciaEducativa);
                if (!existe)
                    throw new KeyNotFoundException($"No existe la Experiencia Educativa con Id {idExperienciaEducativa} para eliminar.");

                bool perteneceAPlan = await _experienciaEducativaRepository.ExperienciaEducativaPerteneceAPlanAsync(idExperienciaEducativa, editarPlanEstudiosDTO.IdPlanEstudios);
                if (!perteneceAPlan)
                    throw new ArgumentException($"La Experiencia Educativa con Id {idExperienciaEducativa} no pertenece al Plan de Estudios.");
            }

            foreach (var experienciaEducativa in editarPlanEstudiosDTO.ExperienciasEditadas)
            {
                validarCamposExperienciaEducativaEditada(experienciaEducativa);
                validarCodigoExperienciaEducativa(experienciaEducativa.Codigo, "El Código de una Experiencia Educativa a editar es inválido.");

                bool existe = await _experienciaEducativaRepository.ExisteExperienciaEducativaPorIdAsync(experienciaEducativa.IdExperienciaEducativa);
                if (!existe)
                    throw new KeyNotFoundException($"No existe ninguna Experiencia Educativa a editar con Id {experienciaEducativa.IdExperienciaEducativa}.");

                bool perteneceAPlan = await _experienciaEducativaRepository.ExperienciaEducativaPerteneceAPlanAsync(experienciaEducativa.IdExperienciaEducativa, editarPlanEstudiosDTO.IdPlanEstudios);
                if (!perteneceAPlan)
                    throw new ArgumentException($"La Experiencia Educativa con Id {experienciaEducativa.IdExperienciaEducativa} no pertenece al Plan de Estudios.");

                bool existeEnOtroPlan = await _experienciaEducativaRepository.ExisteCodigoExperienciaEducativaEnOtroPlanAsync(
                    editarPlanEstudiosDTO.IdPlanEstudios,
                    experienciaEducativa.Codigo.Trim());

                if (existeEnOtroPlan)
                    throw new ArgumentException($"Ya hay una Experiencia Educativa con el Código {experienciaEducativa.Codigo.Trim()} en el sistema.");
            }

            foreach (var experienciaEducativa in editarPlanEstudiosDTO.ExperienciasNuevas)
            {
                validarCamposExperienciaEducativaNueva(experienciaEducativa);
                validarCodigoExperienciaEducativa(experienciaEducativa.Codigo, "El Código de una Experiencia Educativa nueva es inválido.");

                bool existeEnOtroPlan = await _experienciaEducativaRepository.ExisteCodigoExperienciaEducativaEnOtroPlanAsync(
                    editarPlanEstudiosDTO.IdPlanEstudios,
                    experienciaEducativa.Codigo.Trim());

                if (existeEnOtroPlan)
                    throw new ArgumentException($"Ya hay una Experiencia Educativa con el Código {experienciaEducativa.Codigo.Trim()} en el sistema.");
            }

            await validarCodigosDuplicadosDentroDeLaEdicionAsync(editarPlanEstudiosDTO);
        }

        public async Task ValidarIdAsync(int idPlanEstudios)
        {
            if (idPlanEstudios <= 0)
                throw new ValidacionExcepction("La IdPlanEstudios es inválida.", "400");

            bool existe = await _planEstudiosRepository.ExistePorIdAsync(idPlanEstudios);
            if (!existe)
                throw new ValidacionExcepction("No existe ese Plan de Estudios.", "404");
        }

        private async Task validarIdProgramaEducativoAsync(int idProgramaEducativo)
        {
            if (idProgramaEducativo <= 0)
                throw new ArgumentException("El IdProgramaEducativo es inválido.");

            //CAMBIAR POR PROGRAMA EDUCATIVO REPOSITORY
            bool existe = await _planEstudiosRepository.ExisteProgramaEducativoPorIdAsync(idProgramaEducativo);
            if (!existe)
                throw new KeyNotFoundException("No existe ese Programa Educativo.");
        }

        private void validarNombrePlanEstudios(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El Nombre es obligatorio.");
            if (nombre.Trim().Length > 100)
                throw new ArgumentException("El Nombre no puede exceder 100 caracteres.");
        }

        private void validarModalidad(string modalidad)
        {
            if (string.IsNullOrWhiteSpace(modalidad))
                throw new ArgumentException("La Modalidad es obligatoria.");
            if (modalidad.Trim().Length > 100)
                throw new ArgumentException("La Modalidad no puede exceder 100 caracteres.");
            if (!Constantes.Modalidades.Contains(modalidad.Trim()))
                throw new ArgumentException("La Modalidad es inválida.");
        }

        private void validarExperienciasEducativasParaCreacion(List<AgregarExperienciaEducativaDTO> experienciasEducativas)
        {
            ArgumentNullException.ThrowIfNull(experienciasEducativas);

            if (experienciasEducativas.Count == 0)
                throw new ArgumentException("No se puede crear un Plan de Estudios sin Experiencias Educativas.");

            var codigosDuplicados = experienciasEducativas
                .Where(experienciaEducativa => !string.IsNullOrWhiteSpace(experienciaEducativa.Codigo))
                .GroupBy(experienciaEducativa => experienciaEducativa.Codigo.Trim())
                .Any(grupo => grupo.Count() > 1);

            if (codigosDuplicados)
                throw new ArgumentException("Hay Experiencias Educativas con código repetido.");
        }

        private void validarCamposExperienciaEducativaNueva(AgregarExperienciaEducativaDTO experienciaEducativaDTO)
        {
            ArgumentNullException.ThrowIfNull(experienciaEducativaDTO);

            if (string.IsNullOrWhiteSpace(experienciaEducativaDTO.Codigo))
                throw new ArgumentException("El Código es obligatorio en todas las Experiencias Educativas nuevas.");
            if (string.IsNullOrWhiteSpace(experienciaEducativaDTO.Nombre))
                throw new ArgumentException("El Nombre es obligatorio en todas las Experiencias Educativas nuevas.");
            if (string.IsNullOrWhiteSpace(experienciaEducativaDTO.PerfilDocente))
                throw new ArgumentException("El PerfilDocente es obligatorio en todas las Experiencias Educativas nuevas.");
            if (experienciaEducativaDTO.Codigo.Trim().Length > 10)
                throw new ArgumentException("El Código no puede exceder 10 caracteres en todas las Experiencias Educativas nuevas.");
            if (experienciaEducativaDTO.Nombre.Trim().Length > 150)
                throw new ArgumentException("El Nombre no puede exceder 150 caracteres en todas las Experiencias Educativas nuevas.");
        }

        private void validarCamposExperienciaEducativaEditada(DatosExperienciaEducativaDTO experienciaEducativaDTO)
        {
            ArgumentNullException.ThrowIfNull(experienciaEducativaDTO);

            if (experienciaEducativaDTO.IdExperienciaEducativa <= 0)
                throw new ArgumentException("La Id de una Experiencia Educativa a editar es inválida.");
            if (string.IsNullOrWhiteSpace(experienciaEducativaDTO.Codigo))
                throw new ArgumentException("El Código es obligatorio en todas las Experiencias Educativas a editar.");
            if (string.IsNullOrWhiteSpace(experienciaEducativaDTO.Nombre))
                throw new ArgumentException("El Nombre es obligatorio en todas las Experiencias Educativas a editar.");
            if (string.IsNullOrWhiteSpace(experienciaEducativaDTO.PerfilDocente))
                throw new ArgumentException("El PerfilDocente es obligatorio en todas las Experiencias Educativas a editar.");
            if (experienciaEducativaDTO.Codigo.Trim().Length > 10)
                throw new ArgumentException("El Código no puede exceder 10 caracteres.");
            if (experienciaEducativaDTO.Nombre.Trim().Length > 150)
                throw new ArgumentException("El Nombre no puede exceder 150 caracteres.");
        }

        private void validarCodigoExperienciaEducativa(string codigo, string mensajeError)
        {
            if (!regexCodigoExperienciaEducativa.IsMatch(codigo.Trim()))
                throw new ArgumentException(mensajeError);
        }

        private void validarIdsExperienciasEliminadas(List<int> idsExperienciasEliminadas)
        {
            ArgumentNullException.ThrowIfNull(idsExperienciasEliminadas);

            if (idsExperienciasEliminadas.Any(idExperienciaEducativa => idExperienciaEducativa <= 0))
                throw new ArgumentException("La Id de una Experiencia Educativa para eliminar es inválida.");
        }

        private async Task validarCodigosDuplicadosDentroDeLaEdicionAsync(EditarPlanEstudiosDTO editarPlanEstudiosDTO)
        {
            var experienciasActuales = await _experienciaEducativaRepository.ObtenerExperienciasEducativasPorIdPlanEstudiosAsync(editarPlanEstudiosDTO.IdPlanEstudios);
            var idsEliminadas = editarPlanEstudiosDTO.IdsExperienciasEliminadas.ToHashSet();
            var idsEditadas = editarPlanEstudiosDTO.ExperienciasEditadas
                .Select(experienciaEducativa => experienciaEducativa.IdExperienciaEducativa)
                .ToHashSet();

            var codigosFinales = experienciasActuales
                .Where(experienciaEducativa => !idsEliminadas.Contains(experienciaEducativa.IdExperienciaEducativa)
                    && !idsEditadas.Contains(experienciaEducativa.IdExperienciaEducativa))
                .Select(experienciaEducativa => experienciaEducativa.Codigo.Trim())
                .ToList();

            codigosFinales.AddRange(editarPlanEstudiosDTO.ExperienciasEditadas
                .Select(experienciaEducativa => experienciaEducativa.Codigo.Trim()));

            codigosFinales.AddRange(editarPlanEstudiosDTO.ExperienciasNuevas
                .Select(experienciaEducativa => experienciaEducativa.Codigo.Trim()));

            var codigoDuplicado = codigosFinales
                .GroupBy(codigo => codigo)
                .FirstOrDefault(grupo => grupo.Count() > 1)?.Key;

            if (!string.IsNullOrWhiteSpace(codigoDuplicado))
                throw new ArgumentException($"Ya hay una Experiencia Educativa con el Código {codigoDuplicado} en el Plan de Estudios.");
        }
    }
}
