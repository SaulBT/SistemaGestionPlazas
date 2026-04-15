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
            ArgumentNullException.ThrowIfNull(id);

            if (id <= 0)
                throw new ArithmeticException("La Id es inválida");

            bool existe = await _areaAcademicaRepository.ExistePorIdAsync(id);

            if (!existe)
                throw new ArgumentException("No existe ese Área Académica");
        }

        public async Task ValidarEdicionAsync(DatosAreaAcademicaDTO datosAreaAcademicaDTO)
        {
            ArgumentNullException.ThrowIfNull(datosAreaAcademicaDTO);

            await ValidarIdAsync(datosAreaAcademicaDTO.IdAreaAcademica);
            validarCamposEdicion(datosAreaAcademicaDTO);
        }

        private void validarCamposCreacion(CrearAreaAcademicaDTO crearAreaAcademicaDTO)
        {
            if (string.IsNullOrEmpty(crearAreaAcademicaDTO.Nombre.Trim()))
                throw new ArgumentException("El Nombre es obligatorio.");
            if (string.IsNullOrEmpty(crearAreaAcademicaDTO.CalleNumero.Trim()))
                throw new ArgumentException("La CalleNumero es obligatoria.");
            if (string.IsNullOrEmpty(crearAreaAcademicaDTO.Colonia.Trim()))
                throw new ArgumentException("La Colonia es obligatoria.");
            if (string.IsNullOrEmpty(crearAreaAcademicaDTO.Cp.Trim()))
                throw new ArgumentException("El CP es obligatorio.");
            if (string.IsNullOrEmpty(crearAreaAcademicaDTO.Municipio.Trim()))
                throw new ArgumentException("El Municipio es obligatorio.");
            if (string.IsNullOrEmpty(crearAreaAcademicaDTO.Telefono.Trim()))
                throw new ArgumentException("El Teléfono es obligatorio.");
            if (string.IsNullOrEmpty(crearAreaAcademicaDTO.Conmutador.Trim()))
                throw new ArgumentException("El Conmutador es obligatorio.");
            if (string.IsNullOrEmpty(crearAreaAcademicaDTO.Extension.Trim()))
                throw new ArgumentException("La Extensión es obligatoria.");
            if (string.IsNullOrEmpty(crearAreaAcademicaDTO.Fax.Trim()))
                throw new ArgumentException("El Fax es obligatorio.");
        }

        private void validarCamposEdicion(DatosAreaAcademicaDTO datosAreaAcademicaDTO)
        {
            if (string.IsNullOrEmpty(datosAreaAcademicaDTO.Nombre.Trim()))
                throw new ArgumentException("El Nombre es obligatorio.");
            if (string.IsNullOrEmpty(datosAreaAcademicaDTO.CalleNumero.Trim()))
                throw new ArgumentException("La CalleNumero es obligatoria.");
            if (string.IsNullOrEmpty(datosAreaAcademicaDTO.Colonia.Trim()))
                throw new ArgumentException("La Colonia es obligatoria.");
            if (string.IsNullOrEmpty(datosAreaAcademicaDTO.Cp.Trim()))
                throw new ArgumentException("El CP es obligatorio.");
            if (string.IsNullOrEmpty(datosAreaAcademicaDTO.Municipio.Trim()))
                throw new ArgumentException("El Municipio es obligatorio.");
            if (string.IsNullOrEmpty(datosAreaAcademicaDTO.Telefono.Trim()))
                throw new ArgumentException("El Teléfono es obligatorio.");
            if (string.IsNullOrEmpty(datosAreaAcademicaDTO.Conmutador.Trim()))
                throw new ArgumentException("El Conmutador es obligatorio.");
            if (string.IsNullOrEmpty(datosAreaAcademicaDTO.Extension.Trim()))
                throw new ArgumentException("La Extensión es obligatoria.");
            if (string.IsNullOrEmpty(datosAreaAcademicaDTO.Fax.Trim()))
                throw new ArgumentException("El Fax es obligatorio.");
        }
    }
}
