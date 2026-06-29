using SGPla.Models.DTOs.Aviso;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations
{
    public class AvisoService : IAvisoService
    {
        private readonly IAvisoRepository _avisoRepository;
        
        public AvisoService(IAvisoRepository avisoRepository)
        {
            _avisoRepository = avisoRepository;
        }

        public async Task<List<DatosAvisoDTO>> ObtenerTodos()
        {
            throw new NotImplementedException();
        }

        public Task<DatosAvisoDTO?> ObtenerPorID(int idAviso)
        {
            throw new NotImplementedException();
        }

        public Task CrearAviso(CrearAvisoDTO aviso)
        {
            throw new NotImplementedException();
        }

        public Task EliminarAvisoPorId(int idAviso)
        {
            throw new NotImplementedException();
        }

        public Task ActualizarAvisoPorId(EditarAvisoDTO aviso)
        {
            throw new NotImplementedException();
        }
    }
}
