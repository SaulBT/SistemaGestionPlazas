using SGPla.Commons;
using SGPla.Models.DTOs.AreaAcademica;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Validations.Implementations
{
    public class AreaAcademicaValidator : IAreaAcademicaValidator
    {
        private readonly IAreaAcademicaRepository _areaAcademicaRepository;

        public AreaAcademicaValidator(IAreaAcademicaRepository areaAcademicaRepository)
        {
            _areaAcademicaRepository = areaAcademicaRepository;
        }

        public void ValidarCreacion(CrearAreaAcademicaDTO crearAreaAcademicaDTO)
        {
            ArgumentNullException.ThrowIfNull(crearAreaAcademicaDTO);

            validarCamposCreacion(crearAreaAcademicaDTO);
        }

        public async Task ValidarIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidacionExcepction("La Id es inválida", "400");

            bool existe = await _areaAcademicaRepository.ExistePorIdAsync(id);

            if (!existe)
                throw new ValidacionExcepction("No existe esa Área Académica.", "404");
        }

        public async Task ValidarEdicionAsync(DatosAreaAcademicaDTO datosAreaAcademicaDTO)
        {
            ArgumentNullException.ThrowIfNull(datosAreaAcademicaDTO);

            await ValidarIdAsync(datosAreaAcademicaDTO.IdAreaAcademica);
            validarCamposEdicion(datosAreaAcademicaDTO);
        }

        private void validarCamposCreacion(CrearAreaAcademicaDTO crearAreaAcademicaDTO)
        {
            if (string.IsNullOrWhiteSpace(crearAreaAcademicaDTO.Nombre))
                throw new ArgumentException("El Nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(crearAreaAcademicaDTO.Telefono))
                throw new ArgumentException("El Teléfono es obligatorio.");
            if (string.IsNullOrWhiteSpace(crearAreaAcademicaDTO.Extension))
                throw new ArgumentException("La Extensión es obligatoria.");
        }

        private void validarCamposEdicion(DatosAreaAcademicaDTO datosAreaAcademicaDTO)
        {
            if (string.IsNullOrWhiteSpace(datosAreaAcademicaDTO.Nombre))
                throw new ArgumentException("El Nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(datosAreaAcademicaDTO.Telefono))
                throw new ArgumentException("El Teléfono es obligatorio.");
            if (string.IsNullOrWhiteSpace(datosAreaAcademicaDTO.Extension))
                throw new ArgumentException("La Extensión es obligatoria.");
        }
    }
}
