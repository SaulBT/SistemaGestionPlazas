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

            ValidarCamposCreacion(crearUsuarioDTO);
            ValidarFormatoCorreo(crearUsuarioDTO.Correo);
            await ValidarCorreoNoRepetidoAsync(crearUsuarioDTO.Correo);
            ValidarRol(crearUsuarioDTO.Rol);
            await ValidarRelacionRolYDependenciaCreacionAsync(crearUsuarioDTO);
        }

        public async Task ValidarReferenciaAsync(ReferenciaUsuarioDTO referenciaUsuarioDTO)
        {
            ArgumentNullException.ThrowIfNull(referenciaUsuarioDTO);

            ValidarIdUsuarioReferencia(referenciaUsuarioDTO.IdUsuario);
            ValidarRol(referenciaUsuarioDTO.Rol);
            await ValidarExistenciaUsuarioAsync(
                referenciaUsuarioDTO.IdUsuario,
                referenciaUsuarioDTO.Rol,
                "El Usuario no existe.");
        }

        public async Task ValidarEdicionAsync(EditarUsuarioDTO editarUsuarioDTO)
        {
            ArgumentNullException.ThrowIfNull(editarUsuarioDTO);

            ValidarIdUsuarioEdicion(editarUsuarioDTO.IdUsuario);
            ValidarCamposEdicion(editarUsuarioDTO);
            ValidarRol(editarUsuarioDTO.Rol);
            await ValidarExistenciaUsuarioAsync(
                editarUsuarioDTO.IdUsuario,
                editarUsuarioDTO.Rol,
                "No existe ese usuario.");
            await ValidarRelacionRolYDependenciaEdicionAsync(editarUsuarioDTO);
        }

        private static void ValidarCamposCreacion(CrearUsuarioDTO crearUsuarioDTO)
        {
            if (string.IsNullOrWhiteSpace(crearUsuarioDTO.Nombre))
                throw new ArgumentException("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(crearUsuarioDTO.Correo))
                throw new ArgumentException("El correo es obligatorio.");

            if (string.IsNullOrWhiteSpace(crearUsuarioDTO.Cargo))
                throw new ArgumentException("El cargo es obligatorio.");
        }
        private static void ValidarCamposEdicion(EditarUsuarioDTO editarUsuarioDTO)
        {
            if (string.IsNullOrWhiteSpace(editarUsuarioDTO.Nombre))
                throw new ArgumentException("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(editarUsuarioDTO.Cargo))
                throw new ArgumentException("El cargo es obligatorio.");
        }

        private static void ValidarIdUsuarioReferencia(int idUsuario)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El IdUsuario es obligatorio.");
        }

        private static void ValidarIdUsuarioEdicion(int idUsuario)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("La IdUsuario es obligatoria.");
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

        private async Task ValidarExistenciaUsuarioAsync(int idUsuario, string rol, string mensaje)
        {
            if (rol == Constantes.CoordinadorEa)
            {
                var coordinadorEa = await _coordinadorEaRepository.ObtenerPorIdAsync(idUsuario);
                if (coordinadorEa == null)
                    throw new ArgumentException(mensaje);
                return;
            }

            if (rol == Constantes.CoordinadorDgaa)
            {
                var coordinadorDgaa = await _coordinadorDgaaRepository.ObtenerPorIdAsync(idUsuario);
                if (coordinadorDgaa == null)
                    throw new ArgumentException(mensaje);
            }
        }

        private async Task ValidarRelacionRolYDependenciaCreacionAsync(CrearUsuarioDTO crearUsuarioDTO)
        {
            if (crearUsuarioDTO.Rol == Constantes.CoordinadorEa)
            {
                await ValidarCoordinadorEaCreacionAsync(crearUsuarioDTO);
                return;
            }

            if (crearUsuarioDTO.Rol == Constantes.CoordinadorDgaa)
            {
                await ValidarCoordinadorDgaaCreacionAsync(crearUsuarioDTO);
            }
        }

        private async Task ValidarRelacionRolYDependenciaEdicionAsync(EditarUsuarioDTO editarUsuarioDTO)
        {
            if (editarUsuarioDTO.Rol == Constantes.CoordinadorEa)
            {
                await ValidarCoordinadorEaEdicionAsync(editarUsuarioDTO);
                return;
            }

            if (editarUsuarioDTO.Rol == Constantes.CoordinadorDgaa)
            {
                await ValidarCoordinadorDgaaEdicionAsync(editarUsuarioDTO);
            }
        }

        private async Task ValidarCoordinadorEaCreacionAsync(CrearUsuarioDTO crearUsuarioDTO)
        {
            if (crearUsuarioDTO.IdAreaAcademica.HasValue && crearUsuarioDTO.IdAreaAcademica.Value > 0)
                throw new ArgumentException("El rol no corresponde con la dependencia seleccionada.");

            if (!crearUsuarioDTO.IdEntidadAcademica.HasValue || crearUsuarioDTO.IdEntidadAcademica.Value <= 0)
                throw new ArgumentException("La Entidad Académica es obligatoria.");

            bool existeEntidadAcademica = await _entidadAcademicaRepository
                .ExistePorIdAsync(crearUsuarioDTO.IdEntidadAcademica.Value);

            if (!existeEntidadAcademica)
                throw new ArgumentException("No existe esa Entidad Académica.");
        }

        private async Task ValidarCoordinadorDgaaCreacionAsync(CrearUsuarioDTO crearUsuarioDTO)
        {
            if (crearUsuarioDTO.IdEntidadAcademica.HasValue && crearUsuarioDTO.IdEntidadAcademica.Value > 0)
                throw new ArgumentException("El rol no corresponde con la dependencia seleccionada.");

            if (!crearUsuarioDTO.IdAreaAcademica.HasValue || crearUsuarioDTO.IdAreaAcademica.Value <= 0)
                throw new ArgumentException("El Área Académica es obligatoria.");

            bool existeAreaAcademica = await _areaAcademicaRepository
                .ExistePorIdAsync(crearUsuarioDTO.IdAreaAcademica.Value);

            if (!existeAreaAcademica)
                throw new ArgumentException("No existe esa Área Académica.");
        }

        private async Task ValidarCoordinadorEaEdicionAsync(EditarUsuarioDTO editarUsuarioDTO)
        {
            if (editarUsuarioDTO.IdAreaAcademica.HasValue && editarUsuarioDTO.IdAreaAcademica.Value > 0)
                throw new ArgumentException("El rol no corresponde con la dependencia editada.");

            if (editarUsuarioDTO.IdEntidadAcademica.HasValue)
            {
                if (editarUsuarioDTO.IdEntidadAcademica.Value <= 0)
                    throw new ArgumentException("La Entidad Académica es obligatoria.");

                bool existeEntidadAcademica = await _entidadAcademicaRepository
                    .ExistePorIdAsync(editarUsuarioDTO.IdEntidadAcademica.Value);

                if (!existeEntidadAcademica)
                    throw new ArgumentException("No existe esa Entidad Académica.");
            }
        }

        private async Task ValidarCoordinadorDgaaEdicionAsync(EditarUsuarioDTO editarUsuarioDTO)
        {
            if (editarUsuarioDTO.IdEntidadAcademica.HasValue && editarUsuarioDTO.IdEntidadAcademica.Value > 0)
                throw new ArgumentException("El rol no corresponde con la dependencia editada.");

            if (editarUsuarioDTO.IdAreaAcademica.HasValue)
            {
                if (editarUsuarioDTO.IdAreaAcademica.Value <= 0)
                    throw new ArgumentException("El Área Académica es obligatoria.");

                bool existeAreaAcademica = await _areaAcademicaRepository
                    .ExistePorIdAsync(editarUsuarioDTO.IdAreaAcademica.Value);

                if (!existeAreaAcademica)
                    throw new ArgumentException("No existe esa Área Académica.");
            }
        }
    }
}
