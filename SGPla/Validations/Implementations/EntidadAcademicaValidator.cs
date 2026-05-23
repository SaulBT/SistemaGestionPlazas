using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Interfaces;
using SGPla.Commons;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.HttpResults;

namespace SGPla.Validations.Implementations
{
    public class EntidadAcademicaValidator : IEntidadAcademicaValidator
    {
        private readonly IEntidadAcademicaRepository _entidadAcademicaRepository;
        private readonly IAreaAcademicaRepository _areaAcademicaRepository;

        public EntidadAcademicaValidator(
            IEntidadAcademicaRepository entidadAcademicaRepository,
            IAreaAcademicaRepository areaAcademicaRepository)
        {
            _entidadAcademicaRepository = entidadAcademicaRepository;
            _areaAcademicaRepository = areaAcademicaRepository;
        }

        public async Task ValidarCreacionAsync(CrearEntidadAcademicaDTO crearEntidadAcademicaDTO)
        {
            ArgumentNullException.ThrowIfNull(crearEntidadAcademicaDTO);

            await validarIdAreaAcademica(crearEntidadAcademicaDTO.IdAreaAcademica.Value);

            if (crearEntidadAcademicaDTO.Nombre.IsNullOrEmpty())
                throw new ArgumentException("El Nombre es obligatorio.");
            if (crearEntidadAcademicaDTO.CalleNumero.IsNullOrEmpty())
                throw new ArgumentException("La CalleNumero es obligatoria.");
            if (crearEntidadAcademicaDTO.Colonia.IsNullOrEmpty())
                throw new ArgumentException("La Colonia es obligatoria.");
            if (crearEntidadAcademicaDTO.Cp.IsNullOrEmpty())
                throw new ArgumentException("El Cp es obligatorio.");
            if (crearEntidadAcademicaDTO.Municipio.IsNullOrEmpty())
                throw new ArgumentException("El Municipio es obligatorio.");
            if (crearEntidadAcademicaDTO.Telefono.IsNullOrEmpty())
                throw new ArgumentException("El Teléfono es obligatorio.");
            if (crearEntidadAcademicaDTO.Conmutador.IsNullOrEmpty())
                throw new ArgumentException("El Conmutador es obligatorio.");
            if (crearEntidadAcademicaDTO.Extension.IsNullOrEmpty())
                throw new ArgumentException("La Extensión es obligatoria.");
            if (crearEntidadAcademicaDTO.Fax.IsNullOrEmpty())
                throw new ArgumentException("El Fax es obligatorio.");
            if (crearEntidadAcademicaDTO.Region.IsNullOrEmpty())
                throw new ArgumentException("La Region es obligatoria.");

            await validarClaveAsync(crearEntidadAcademicaDTO.Clave, crearEntidadAcademicaDTO.Region);
        }

        public async Task ValidarEdicionAsync(DatosEntidadAcademicaDTO datosEntidadAcademicaDTO)
        {
            ArgumentNullException.ThrowIfNull(datosEntidadAcademicaDTO);

            await ValidarIdAsync(datosEntidadAcademicaDTO.IdEntidadAcademica);
            await validarIdAreaAcademica(datosEntidadAcademicaDTO.IdAreaAcademica);

            if (datosEntidadAcademicaDTO.Nombre.IsNullOrEmpty())
                throw new ArgumentException("El Nombre es obligatorio.");
            if (datosEntidadAcademicaDTO.CalleNumero.IsNullOrEmpty())
                throw new ArgumentException("La CalleNumero es obligatoria.");
            if (datosEntidadAcademicaDTO.Colonia.IsNullOrEmpty())
                throw new ArgumentException("La Colonia es obligatoria.");
            if (datosEntidadAcademicaDTO.Cp.IsNullOrEmpty())
                throw new ArgumentException("El Cp es obligatorio.");
            if (datosEntidadAcademicaDTO.Municipio.IsNullOrEmpty())
                throw new ArgumentException("El Municipio es obligatorio.");
            if (datosEntidadAcademicaDTO.Telefono.IsNullOrEmpty())
                throw new ArgumentException("El Teléfono es obligatorio.");
            if (datosEntidadAcademicaDTO.Conmutador.IsNullOrEmpty())
                throw new ArgumentException("El Conmutador es obligatorio.");
            if (datosEntidadAcademicaDTO.Extension.IsNullOrEmpty())
                throw new ArgumentException("La Extensión es obligatoria.");
            if (datosEntidadAcademicaDTO.Fax.IsNullOrEmpty())
                throw new ArgumentException("El Fax es obligatorio.");
            if (datosEntidadAcademicaDTO.Region.IsNullOrEmpty())
                throw new ArgumentException("La Region es obligatoria.");

            await validarClaveAsync(datosEntidadAcademicaDTO.Clave, datosEntidadAcademicaDTO.Region, datosEntidadAcademicaDTO.IdEntidadAcademica);
        }

        public void ValidarIndice(int indice)
        {
            ArgumentNullException.ThrowIfNull(indice);

            if (indice <= 0)
                throw new ArgumentException("El Índice es inválido");
        }

        public async Task ValidarIdAsync(int id)
        {
            validarEstadoId(id, Constantes.ID_ENTIDAD_ACADEMICA);

            bool existe = await _entidadAcademicaRepository.ExistePorIdAsync(id);
            if (!existe)
                throw new KeyNotFoundException("No existe esa Entidad Académica.");
        }

        //IdAreaAcademica
        private async Task validarIdAreaAcademica(int id)
        {
            validarEstadoId(id, Constantes.ID_AREA_ACADEMICA);

            bool existe = await _areaAcademicaRepository.ExistePorIdAsync(id);
            if (!existe)
                throw new KeyNotFoundException("No existe esa Área Académica.");
        }

        private void validarEstadoId(int id, string tipo)
        {
            if (id <= 0)
                throw new ArgumentException($"La {tipo} es inválida.");
        }

        //Clave
        private async Task validarClaveAsync(string clave, string region, int? id = -1)
        {
            if (clave.IsNullOrEmpty())
                throw new ArgumentException("La Clave es obligatoria.");

            bool soloNumeros = clave.All(char.IsDigit);
            if (clave.Count() != 5 || !soloNumeros)
                throw new ArgumentException("La Clave es inválida.");
            
            if (id >= 0)
            {
                bool existe = await _entidadAcademicaRepository.ExistePorClaveAsync(clave, id.Value);
                if (existe)
                    throw new ArgumentException("La Clave ya está en uso.");
            }
            

            bool error = false;
            switch (region)
            {
                case Constantes.REGION_XALAPA:
                    if (!clave.StartsWith("1"))
                        error = true;
                    break;
                case Constantes.REGION_VERACRUZ:
                    if (!clave.StartsWith("2"))
                        error = true;
                    break;
                case Constantes.REGION_ORIZABA:
                    if (!clave.StartsWith("3"))
                        error = true;
                    break;
                case Constantes.REGION_POZARICA_TUXPAN:
                    if (!clave.StartsWith("4"))
                        error = true;
                    break;
                case Constantes.REGION_COATZACOALCOS_MINATITLAN:
                    if (!clave.StartsWith("5"))
                        error = true;
                    break;
                default:
                    error = true;
                    break;
            }
            if (error)
                throw new ArgumentException("La Clave no coincide con la Región seleccionada.");
        }
    }
}
