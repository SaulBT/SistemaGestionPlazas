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
                throw new ArithmeticException("La Id es inválida");

            bool existe = await _areaAcademicaRepository.ExistePorIdAsync(id);

            if (!existe)
                throw new ArgumentException("No existe esa Área Académica.");
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
            if (string.IsNullOrWhiteSpace(crearAreaAcademicaDTO.CalleNumero))
                throw new ArgumentException("La CalleNumero es obligatoria.");
            if (string.IsNullOrWhiteSpace(crearAreaAcademicaDTO.Colonia))
                throw new ArgumentException("La Colonia es obligatoria.");
            if (string.IsNullOrWhiteSpace(crearAreaAcademicaDTO.Cp))
                throw new ArgumentException("El CP es obligatorio.");
            if (string.IsNullOrWhiteSpace(crearAreaAcademicaDTO.Municipio))
                throw new ArgumentException("El Municipio es obligatorio.");
            if (string.IsNullOrWhiteSpace(crearAreaAcademicaDTO.Telefono))
                throw new ArgumentException("El Teléfono es obligatorio.");
            if (string.IsNullOrWhiteSpace(crearAreaAcademicaDTO.Conmutador))
                throw new ArgumentException("El Conmutador es obligatorio.");
            if (string.IsNullOrWhiteSpace(crearAreaAcademicaDTO.Extension))
                throw new ArgumentException("La Extensión es obligatoria.");
            if (string.IsNullOrWhiteSpace(crearAreaAcademicaDTO.Fax))
                throw new ArgumentException("El Fax es obligatorio.");
        }

        private void validarCamposEdicion(DatosAreaAcademicaDTO datosAreaAcademicaDTO)
        {
            if (string.IsNullOrWhiteSpace(datosAreaAcademicaDTO.Nombre))
                throw new ArgumentException("El Nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(datosAreaAcademicaDTO.CalleNumero))
                throw new ArgumentException("La CalleNumero es obligatoria.");
            if (string.IsNullOrWhiteSpace(datosAreaAcademicaDTO.Colonia))
                throw new ArgumentException("La Colonia es obligatoria.");
            if (string.IsNullOrWhiteSpace(datosAreaAcademicaDTO.Cp))
                throw new ArgumentException("El CP es obligatorio.");
            if (string.IsNullOrWhiteSpace(datosAreaAcademicaDTO.Municipio))
                throw new ArgumentException("El Municipio es obligatorio.");
            if (string.IsNullOrWhiteSpace(datosAreaAcademicaDTO.Telefono))
                throw new ArgumentException("El Teléfono es obligatorio.");
            if (string.IsNullOrWhiteSpace(datosAreaAcademicaDTO.Conmutador))
                throw new ArgumentException("El Conmutador es obligatorio.");
            if (string.IsNullOrWhiteSpace(datosAreaAcademicaDTO.Extension))
                throw new ArgumentException("La Extensión es obligatoria.");
            if (string.IsNullOrWhiteSpace(datosAreaAcademicaDTO.Fax))
                throw new ArgumentException("El Fax es obligatorio.");
        }
    }
}
