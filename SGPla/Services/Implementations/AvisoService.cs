using SGPla.Models.DTOs.Aviso;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations
{
    public class AvisoService : IAvisoService
    {
        private readonly IAvisoRepository _avisoRepository;
        private readonly IProgramacionAcademicaRepository _programacionAcademicaRepository;
        private readonly IOfertaRepository _ofertaRepository;

        public AvisoService(IAvisoRepository avisoRepository, 
            IProgramacionAcademicaRepository programacionAcademicaRepository, 
            IOfertaRepository ofertaRepository)
        {
            _avisoRepository = avisoRepository;
            _programacionAcademicaRepository = programacionAcademicaRepository;
            _ofertaRepository = ofertaRepository;
        }

        public async Task<(List<ListaAvisosDTO> items, int total)> ObtenerTodosAvisosAsync(FiltroAvisosDTO filtroDTO)
        {
            throw new NotImplementedException();
        }

        public Task<DatosAvisoDTO> ObtenerAvisoPorIDAsync(int idAviso)
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

        //DGAA
        public async Task RevisarAvisoAsync(RevisionDTO revisionDTO)
        {
            throw new NotImplementedException();
        }

        public async Task ArchivarAvisoAsync(int idAviso)
        {
            throw new NotImplementedException();
        }

        //Datos necesarios
        public async Task<List<OfertaPlanEstudiosAvisoDTO>> ObtenerPlanesConOfertasAviso(int idEntidadAcademica, int idPeriodo, int idArticulo)
        {
            var planes = await _ofertaRepository.ObtenerPlanesEstudioCrearAviso(idEntidadAcademica, idPeriodo, idArticulo);

            return planes;
        }
    }
}
