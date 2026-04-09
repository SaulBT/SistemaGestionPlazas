using SGPla.Commons;
using SGPla.Models.DTOs.Usuarios;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Interfaz;
using System.Net.Mail;

namespace SGPla.Validations.Implementation
{
    public class UsuarioValidator : IUsuarioValidator
    {
        private readonly ICoordinadorEaRepository _coordinadorEaRepository;
        private readonly ICoordinadorDgaaRepository _coordinadorDgaaRepository;
        private readonly IAreaAcademicaRepository _areaAcademicaRepository;
        private readonly IEntidadAcademicaRepository _entidadAcademicaRepository;

        public UsuarioValidator(
            ICoordinadorEaRepository coordinadorEaRepository,
            ICoordinadorDgaaRepository coordinadorDgaaRepository,
            IAreaAcademicaRepository areaAcademicaRepository,
            IEntidadAcademicaRepository entidadAcademicaRepository)
        {
            _coordinadorEaRepository = coordinadorEaRepository;
            _coordinadorDgaaRepository = coordinadorDgaaRepository;
            _areaAcademicaRepository = areaAcademicaRepository;
            _entidadAcademicaRepository = entidadAcademicaRepository;
        }

        public async Task ValidarCreacionAsync(CrearUsuarioDTO crearUsuarioDTO)
        {
            ArgumentNullException.ThrowIfNull(crearUsuarioDTO);

            ValidarCampos(crearUsuarioDTO);
            ValidarFormatoCorreo(crearUsuarioDTO.Correo);
            await ValidarCorreoNoRepetidoAsync(crearUsuarioDTO.Correo);

            ValidarRol(crearUsuarioDTO.Rol);
            await ValidarRelacionRolYDependenciaAsync(crearUsuarioDTO);
        }

        private void ValidarCampos(CrearUsuarioDTO crearUsuarioDTO)
        {
            if (string.IsNullOrWhiteSpace(crearUsuarioDTO.Nombre))
                throw new ArgumentException("El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(crearUsuarioDTO.Correo))
                throw new ArgumentException("El correo es obligatorio.");
            if (string.IsNullOrWhiteSpace(crearUsuarioDTO.Cargo))
                throw new ArgumentException("El cargo es obligatorio.");
        }

        private void ValidarFormatoCorreo(string correo)
        {
            if (!MailAddress.TryCreate(correo, out _))
                throw new ArgumentException("El formato del correo no es válido.");
        }

        private async Task ValidarCorreoNoRepetidoAsync(string correo)
        {
            bool existeEnCoordinadorEa = await _coordinadorEaRepository.ExisteCorreoAsync(correo);
            if (existeEnCoordinadorEa)
                throw new ArgumentException("El correo ya está en uso.");

            bool existeEnCoordinadorDgaa = await _coordinadorDgaaRepository.ExisteCorreoAsync(correo);
            if (existeEnCoordinadorDgaa)
                throw new ArgumentException("El correo ya está en uso.");
        }

        private void ValidarRol(string rol)
        {
            if (string.IsNullOrWhiteSpace(rol))
                throw new ArgumentException("El rol es obligatorio.");

            if (rol != Constantes.CoordinadorEa &&
                rol != Constantes.CoordinadorDgaa)
            {
                throw new ArgumentException("El rol no es válido.");
            }
        }

        private async Task ValidarRelacionRolYDependenciaAsync(CrearUsuarioDTO CrearUsuarioDTO)
        {
            if (CrearUsuarioDTO.Rol == Constantes.CoordinadorEa)
            {
                await ValidarCoordinadorEaAsync(CrearUsuarioDTO);
                return;
            }

            if (CrearUsuarioDTO.Rol == Constantes.CoordinadorDgaa)
            {
                await ValidarCoordinadorDgaaAsync(CrearUsuarioDTO);
            }
        }

        private async Task ValidarCoordinadorEaAsync(CrearUsuarioDTO CrearUsuarioDTO)
        {
            if (!CrearUsuarioDTO.IdEntidadAcademica.HasValue || CrearUsuarioDTO.IdEntidadAcademica.Value <= 0)
                throw new ArgumentException("La Entidad Académica es obligatoria");

            if (CrearUsuarioDTO.IdAreaAcademica.HasValue && CrearUsuarioDTO.IdAreaAcademica.Value > 0)
                throw new ArgumentException("Rol incorrecto.");

            bool existeEntidadAcademica = await _entidadAcademicaRepository
                .ExistePorIdAsync(CrearUsuarioDTO.IdEntidadAcademica.Value);

            if (!existeEntidadAcademica)
                throw new ArgumentException("No existe esa Entidad Académica.");
        }

        private async Task ValidarCoordinadorDgaaAsync(CrearUsuarioDTO CrearUsuarioDTO)
        {
            if (!CrearUsuarioDTO.IdAreaAcademica.HasValue || CrearUsuarioDTO.IdAreaAcademica.Value <= 0)
                throw new ArgumentException("El Area Académica es obligatoria.");

            if (CrearUsuarioDTO.IdEntidadAcademica.HasValue && CrearUsuarioDTO.IdEntidadAcademica.Value > 0)
                throw new ArgumentException("Rol incorrecto.");

            bool existeAreaAcademica = await _areaAcademicaRepository
                .ExistePorIdAsync(CrearUsuarioDTO.IdAreaAcademica.Value);

            if (!existeAreaAcademica)
                throw new ArgumentException("No existe esa Área Académica.");
        }
    }
}
