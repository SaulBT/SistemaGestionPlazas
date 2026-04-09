using SGPla.Commons;
using SGPla.Models;
using SGPla.Models.DTOs.Usuarios;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using SGPla.Validations.Interfaz;

namespace SGPla.Services.Implementations
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ICoordinadorEaRepository _coordinadorEaRepository;
        private readonly ICoordinadorDgaaRepository _coordinadorDgaaRepository;
        private readonly IUsuarioValidator _usuarioValidator;

        public UsuarioService(
            ICoordinadorEaRepository coordinadorEaRepository,
            ICoordinadorDgaaRepository coordinadorDgaaRepository,
            IUsuarioValidator usuarioValidator)
        {
            _coordinadorEaRepository = coordinadorEaRepository;
            _coordinadorDgaaRepository = coordinadorDgaaRepository;
            _usuarioValidator = usuarioValidator;
        }

        //Puede que sea necesario cambiar el retorno, porque la Id por sí sola no dice el rol
        public async Task<int> CrearAsync(CrearUsuarioDTO dto)
        {
            await _usuarioValidator.ValidarCreacionAsync(dto);

            return dto.Rol switch
            {
                Constantes.CoordinadorEa => await CrearCoordinadorEaAsync(dto),
                Constantes.CoordinadorDgaa => await CrearCoordinadorDgaaAsync(dto),
                _ => throw new ArgumentException("El rol especificado no es válido.")
            };
        }

        public async Task<List<ListaUsuarioDTO>> ObtenerTodosAsync()
        {
            var coordinadoresEa = await _coordinadorEaRepository.ObtenerTodosAsync();
            var coordinadoresDgaa = await _coordinadorDgaaRepository.ObtenerTodosAsync();

            var listaCoordinadoresEa = coordinadoresEa.Select(MapearCoordinadorEaAListaDTO);
            var listaCoordinadoresDgaa = coordinadoresDgaa.Select(MapearCoordinadorDgaaAListaDTO);

            return listaCoordinadoresEa
                .Concat(listaCoordinadoresDgaa)
                .OrderBy(u => u.Nombre)
                .ToList();
        }

        public async Task<List<ListaUsuarioDTO>> ObtenerPorFiltroAsync(FiltrosUsuarioDTO filtro)
        {
            if (filtro == null)
                throw new ArgumentNullException(nameof(filtro));

            if (filtro.Rol == Constantes.CoordinadorEa)
            {
                return await ObtenerCoordinadoresEaPorFiltroAsync(filtro);
            }

            if (filtro.Rol == Constantes.CoordinadorDgaa)
            {
                return await ObtenerCoordinadoresDgaaPorFiltroAsync(filtro);
            }

            return await ObtenerTodosPorFiltroAsync(filtro);
        }

        public async Task<DetallesUsuarioDTO?> ObtenerPorIdAsync(ReferenciaUsuarioDTO dto)
        {
            if (dto.Rol == Constantes.CoordinadorEa)
            {
                var coordinadorEa = await _coordinadorEaRepository.ObtenerPorIdAsync(dto.IdUsuario);

                if (coordinadorEa == null)
                    return null;

                return MapearCoordinadorEaADetallesDTO(coordinadorEa);
            }

            if (dto.Rol == Constantes.CoordinadorDgaa)
            {
                var coordinadorDgaa = await _coordinadorDgaaRepository.ObtenerPorIdAsync(dto.IdUsuario);

                if (coordinadorDgaa == null)
                    return null;

                return MapearCoordinadorDgaaADetallesDTO(coordinadorDgaa);
            }

            throw new ArgumentException("El rol especificado no es válido.");
        }

        //Quizás también convenga retornar el objeto editado.
        public async Task EditarAsync(EditarUsuarioDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Rol == Constantes.CoordinadorEa)
            {
                await EditarCoordinadorEaAsync(dto);
                return;
            }

            if (dto.Rol == Constantes.CoordinadorDgaa)
            {
                await EditarCoordinadorDgaaAsync(dto);
                return;
            }

            throw new ArgumentException("El rol especificado no es válido.");
        }

        public async Task EliminarAsync(ReferenciaUsuarioDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Rol == Constantes.CoordinadorEa)
            {
                await EliminarCoordinadorEaAsync(dto.IdUsuario);
                return;
            }

            if (dto.Rol == Constantes.CoordinadorDgaa)
            {
                await EliminarCoordinadorDgaaAsync(dto.IdUsuario);
                return;
            }

            throw new ArgumentException("El rol especificado no es válido.");
        }

        //Crear usuarios
        private async Task<int> CrearCoordinadorEaAsync(CrearUsuarioDTO dto)
        {
            if (!dto.IdEntidadAcademica.HasValue)
                throw new ArgumentException("IdEntidadAcademica es obligatorio para crear un Coordinador de Entidad Académica.");

            var coordinadorEa = new CoordinadorEa
            {
                Nombre = dto.Nombre,
                Correo = dto.Correo,
                Cargo = dto.Cargo,
                IdEntidadAcademica = dto.IdEntidadAcademica.Value
            };

            return await _coordinadorEaRepository.CrearAsync(coordinadorEa);
        }

        private async Task<int> CrearCoordinadorDgaaAsync(CrearUsuarioDTO dto)
        {
            if (!dto.IdAreaAcademica.HasValue)
                throw new ArgumentException("IdAreaAcademica es obligatorio para crear un Coordinador de Área Académica.");

            var coordinadorDgaa = new CoordinadorDgaa
            {
                Nombre = dto.Nombre,
                Correo = dto.Correo,
                Cargo = dto.Cargo,
                IdAreaAcademica = dto.IdAreaAcademica.Value
            };

            return await _coordinadorDgaaRepository.CrearAsync(coordinadorDgaa);
        }

        //Obtener por filtros
        private async Task<List<ListaUsuarioDTO>> ObtenerCoordinadoresEaPorFiltroAsync(FiltrosUsuarioDTO filtro)
        {
            var coordinadoresEa = await _coordinadorEaRepository.ObtenerPorFiltrosAsync(
                filtro.Region,
                filtro.IdAreaAcademica,
                filtro.IdEntidadAcademica);

            return coordinadoresEa
                .Select(MapearCoordinadorEaAListaDTO)
                .OrderBy(u => u.Nombre)
                .ToList();
        }

        private async Task<List<ListaUsuarioDTO>> ObtenerCoordinadoresDgaaPorFiltroAsync(FiltrosUsuarioDTO filtro)
        {
            var coordinadoresDgaa = await _coordinadorDgaaRepository.ObtenerPorFiltrosAsync(
                filtro.IdAreaAcademica);

            return coordinadoresDgaa
                .Select(MapearCoordinadorDgaaAListaDTO)
                .OrderBy(u => u.Nombre)
                .ToList();
        }

        private async Task<List<ListaUsuarioDTO>> ObtenerTodosPorFiltroAsync(FiltrosUsuarioDTO filtro)
        {
            var coordinadoresEa = await _coordinadorEaRepository.ObtenerPorFiltrosAsync(
                filtro.Region,
                filtro.IdAreaAcademica,
                filtro.IdEntidadAcademica);

            var coordinadoresDgaa = await _coordinadorDgaaRepository.ObtenerPorFiltrosAsync(
                filtro.IdAreaAcademica);

            var listaCoordinadoresEa = coordinadoresEa.Select(MapearCoordinadorEaAListaDTO);
            var listaCoordinadoresDgaa = coordinadoresDgaa.Select(MapearCoordinadorDgaaAListaDTO);

            return listaCoordinadoresEa
                .Concat(listaCoordinadoresDgaa)
                .OrderBy(u => u.Nombre)
                .ToList();
        }

        //Editar
        private async Task EditarCoordinadorEaAsync(EditarUsuarioDTO dto)
        {
            var coordinadorEa = await _coordinadorEaRepository.ObtenerPorIdAsync(dto.IdUsuario);

            if (coordinadorEa == null)
                throw new InvalidOperationException("No se encontró el Coordinador de Entidad Académica.");

            coordinadorEa.Nombre = dto.Nombre;
            coordinadorEa.Cargo = dto.Cargo;

            if (dto.IdEntidadAcademica.HasValue)
            {
                coordinadorEa.IdEntidadAcademica = dto.IdEntidadAcademica.Value;
            }

            await _coordinadorEaRepository.ActualizarAsync(coordinadorEa);
        }

        private async Task EditarCoordinadorDgaaAsync(EditarUsuarioDTO dto)
        {
            var coordinadorDgaa = await _coordinadorDgaaRepository.ObtenerPorIdAsync(dto.IdUsuario);

            if (coordinadorDgaa == null)
                throw new InvalidOperationException("No se encontró el Coordinador de Área Académica.");

            coordinadorDgaa.Nombre = dto.Nombre;
            coordinadorDgaa.Cargo = dto.Cargo;

            if (dto.IdAreaAcademica.HasValue)
            {
                coordinadorDgaa.IdAreaAcademica = dto.IdAreaAcademica.Value;
            }

            await _coordinadorDgaaRepository.ActualizarAsync(coordinadorDgaa);
        }

        //Eliminar
        private async Task EliminarCoordinadorEaAsync(int idUsuario)
        {
            var coordinadorEa = await _coordinadorEaRepository.ObtenerPorIdAsync(idUsuario);

            if (coordinadorEa == null)
                throw new InvalidOperationException("No se encontró el Coordinador de Entidad Académica.");

            await _coordinadorEaRepository.EliminarAsync(coordinadorEa);
        }
        private async Task EliminarCoordinadorDgaaAsync(int idUsuario)
        {
            var coordinadorDgaa = await _coordinadorDgaaRepository.ObtenerPorIdAsync(idUsuario);

            if (coordinadorDgaa == null)
                throw new InvalidOperationException("No se encontró el Coordinador de Área Académica.");

            await _coordinadorDgaaRepository.EliminarAsync(coordinadorDgaa);
        }

        //Mapeos
        private ListaUsuarioDTO MapearCoordinadorEaAListaDTO(CoordinadorEa coordinadorEa)
        {
            return new ListaUsuarioDTO
            {
                IdUsuario = coordinadorEa.IdCoordinadorEa,
                Nombre = coordinadorEa.Nombre,
                Correo = coordinadorEa.Correo,
                Cargo = coordinadorEa.Cargo,
                Rol = Constantes.CoordinadorEa,
                NombreEntidadAcademica = coordinadorEa.IdEntidadAcademicaNavigation?.Nombre,
                NombreAreaAcademica = coordinadorEa.IdEntidadAcademicaNavigation?.IdAreaAcademicaNavigation?.Nombre,
                Region = coordinadorEa.IdEntidadAcademicaNavigation?.Region
            };
        }

        private ListaUsuarioDTO MapearCoordinadorDgaaAListaDTO(CoordinadorDgaa coordinadorDgaa)
        {
            return new ListaUsuarioDTO
            {
                IdUsuario = coordinadorDgaa.IdCoordinadorDgaa,
                Nombre = coordinadorDgaa.Nombre,
                Correo = coordinadorDgaa.Correo,
                Cargo = coordinadorDgaa.Cargo,
                Rol = Constantes.CoordinadorDgaa,
                NombreEntidadAcademica = null,
                NombreAreaAcademica = coordinadorDgaa.IdAreaAcademicaNavigation?.Nombre,
                Region = null
            };
        }

        private DetallesUsuarioDTO MapearCoordinadorEaADetallesDTO(CoordinadorEa coordinadorEa)
        {
            return new DetallesUsuarioDTO
            {
                IdUsuario = coordinadorEa.IdCoordinadorEa,
                Nombre = coordinadorEa.Nombre,
                Correo = coordinadorEa.Correo,
                Cargo = coordinadorEa.Cargo,
                Rol = Constantes.CoordinadorEa,
                IdAreaAcademica = coordinadorEa.IdEntidadAcademicaNavigation?.IdAreaAcademica,
                NombreAreaAcademica = coordinadorEa.IdEntidadAcademicaNavigation?.IdAreaAcademicaNavigation?.Nombre,
                IdEntidadAcademica = coordinadorEa.IdEntidadAcademica,
                NombreEntidadAcademica = coordinadorEa.IdEntidadAcademicaNavigation?.Nombre,
                Region = coordinadorEa.IdEntidadAcademicaNavigation?.Region
            };
        }

        private DetallesUsuarioDTO MapearCoordinadorDgaaADetallesDTO(CoordinadorDgaa coordinadorDgaa)
        {
            return new DetallesUsuarioDTO
            {
                IdUsuario = coordinadorDgaa.IdCoordinadorDgaa,
                Nombre = coordinadorDgaa.Nombre,
                Correo = coordinadorDgaa.Correo,
                Cargo = coordinadorDgaa.Cargo,
                Rol = Constantes.CoordinadorDgaa,
                IdAreaAcademica = coordinadorDgaa.IdAreaAcademica,
                NombreAreaAcademica = coordinadorDgaa.IdAreaAcademicaNavigation?.Nombre,
                IdEntidadAcademica = null,
                NombreEntidadAcademica = null,
                Region = null
            };
        }
    }
}
