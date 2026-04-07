using SGPla.Commons;
using SGPla.Data;
using SGPla.Models;
using SGPla.Models.DTOs.Usuario;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations
{
    public class UsuarioService : IUsuarioService
    {
        private readonly GestionDePlazasDbContext _context;

        public UsuarioService(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<int> CrearAsync(CrearUsuarioDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return dto.Rol switch
            {
                Constantes.RolCoordinadorEa => await CrearCoordinadorEaAsync(dto),
                Constantes.RolCoordinadorDgaa => await CrearCoordinadorDgaaAsync(dto),
                _ => throw new ArgumentException("El rol especificado no es válido.")
            };
        }

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

            _context.CoordinadorEas.Add(coordinadorEa);
            await _context.SaveChangesAsync();

            return coordinadorEa.IdCoordinadorEa;
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

            _context.CoordinadorDgaas.Add(coordinadorDgaa);
            await _context.SaveChangesAsync();

            return coordinadorDgaa.IdCoordinadorDgaa;
        }

        public Task<List<ListaUsuarioDTO>> ObtenerTodosAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<ListaUsuarioDTO>> ObtenerPorFiltroAsync(FiltrosUsuarioDTO filtro)
        {
            throw new NotImplementedException();
        }

        public Task<DetallesUsuarioDTO?> ObtenerPorIdAsync(int idUsuario)
        {
            throw new NotImplementedException();
        }

        public Task EditarAsync(EditarUsuarioDTO dto)
        {
            throw new NotImplementedException();
        }

        public Task EliminarAsync(int idUsuario)
        {
            throw new NotImplementedException();
        }
    }
}
