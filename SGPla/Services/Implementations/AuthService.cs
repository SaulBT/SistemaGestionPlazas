using SGPla.Commons;
using SGPla.Models.DTOs.Auth;
using SGPla.Repositories.Implementations;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ILdapAuthService _ldapService;
        private readonly ICoordinadorDgaaRepository _coordinadorDgaaRepository;
        private readonly ICoordinadorEaRepository _coordinadorEaRepository;

        public AuthService(ILdapAuthService ldapService, ICoordinadorDgaaRepository coordinadorDgaaRepository, ICoordinadorEaRepository coordinadorEaRepository)
        {
            _ldapService = ldapService;
            _coordinadorDgaaRepository = coordinadorDgaaRepository;
            _coordinadorEaRepository = coordinadorEaRepository;
        }

        public async Task<ResultadoAutenticacion> LoginAsync(string username, string password)
        {
            var correo = NormalizarCorreo(username);

            if (!_ldapService.Autenticar(correo, password))
                return ResultadoAutenticacion.Fallido("Usuario o contraseña incorrectos.");

            var superUsuario = await _coordinadorDgaaRepository.ObtenerSuperUsuarioPorCorreoAsync(correo);
            var coordinador = await _coordinadorDgaaRepository.ExisteCorreoAsync(correo);
            var entidad = await _coordinadorEaRepository.ExisteCorreoAsync(correo);

            if (superUsuario is null && !coordinador && !entidad)
                return ResultadoAutenticacion.Fallido("La cuenta no está registrada en el sistema.");

            UsuarioDTO usuarioDTO = new UsuarioDTO();

            if (superUsuario is not null)
            {
                usuarioDTO.Correo = superUsuario.Correo;
                usuarioDTO.Id = superUsuario.IdSuperUsuario;
                usuarioDTO.NombreCompleto = superUsuario.Nombre;
                usuarioDTO.Rol = Constantes.SUPERUSUARIO;
            }
            else if (coordinador)
            {
                var usuario = await _coordinadorDgaaRepository.ObtenerPorCorreoAsync(correo);

                usuarioDTO.Correo = usuario.Correo;
                usuarioDTO.Id = usuario.IdCoordinadorDgaa;
                usuarioDTO.NombreCompleto = usuario.Nombre;
                usuarioDTO.Rol = Constantes.COORDINADOR_DGAA;
            }
            else if (entidad)
            {
                var usuario = await _coordinadorEaRepository.ObtenerPorCorreoAsync(correo);

                usuarioDTO.Correo = usuario.Correo;
                usuarioDTO.Id = usuario.IdCoordinadorEa;
                usuarioDTO.NombreCompleto = usuario.Nombre;
                usuarioDTO.Rol = Constantes.COORDINADOR_EA;
                usuarioDTO.EntidadAcademicaId = usuario.IdEntidadAcademica;
            }

            return ResultadoAutenticacion.Ok(usuarioDTO);
        }

        private static string NormalizarCorreo(string username)
        {
            username = username.Trim();
            return username.Contains('@') ? username : $"{username}@uv.mx";
        }
    }
}
