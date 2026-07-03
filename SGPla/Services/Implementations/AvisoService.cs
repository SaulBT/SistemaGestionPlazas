using SGPla.Mappers;
using SGPla.Models;
using SGPla.Models.DTOs.Aviso;
using SGPla.Models.DTOs.PeriodoEscolar;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations
{
    public class AvisoService : IAvisoService
    {
        private readonly IAvisoRepository _avisoRepository;
        private readonly IPeriodoEscolarRepository _periodoEscolarRepository;

        public AvisoService(
            IAvisoRepository avisoRepository,
            IPeriodoEscolarRepository periodoEscolarRepository)
        {
            _avisoRepository = avisoRepository;
            _periodoEscolarRepository = periodoEscolarRepository;
        }

        public async Task<(List<ListaAvisosDTO> items, int total)> ObtenerTodosAvisosAsync(FiltroAvisosDTO filtroDTO)
        {
            var avisos = await _avisoRepository.ObtenerTodosAsync(filtroDTO);

            var items = (await Task.WhenAll(avisos.Select(mapearAListaAvisoDTOAsync))).ToList();
            var total = await _avisoRepository.ContarAsync(filtroDTO);

            return (items, total);
        }

        public async Task<DatosAvisoDTO> ObtenerAvisoPorIDAsync(int idAviso)
        {
            var aviso = await _avisoRepository.ObtenerPorIDAsync(idAviso);
            var periodoDTO = obtenerPeriodoDTOAsync(aviso.IdPeriodo);

            var avisoDTO = new DatosAvisoDTO
            {
                IdAviso = aviso.IdAviso,
                IdEntidadAcademica = aviso.IdEntidadAcademica,
                IdPeriodo = aviso.IdPeriodo,
                IdArticulo = aviso.IdArticulo,
                IdArchivoOriginal = aviso.IdArchivoOriginal,
                IdArchivoFirmado = aviso.IdArchivoFirmado
            }
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

        private async Task<ListaAvisosDTO> mapearAListaAvisoDTOAsync(Aviso aviso)
        {
            var periodoDTO = await obtenerPeriodoDTOAsync(aviso.IdPeriodo);
            var avisoDTO = new ListaAvisosDTO
            {
                IdAviso = aviso.IdAviso,
                IdEntidadAcademica = aviso.IdEntidadAcademica,
                IdPeriodo = aviso.IdPeriodo,
                IdArticulo = aviso.IdArticulo,
                NombreEntidadAcademica = aviso.IdEntidadAcademicaNavigation.Nombre,
                Folio = aviso.Folio,
                Periodo = periodoDTO.PeriodoMostrar,
                Articulo = aviso.IdArticuloNavigation.Numero,
                FechaCreacion = aviso.FechaCreacion.ToString("dd/MM/yyyy"),
                Estado = aviso.Estado
            };

            return avisoDTO;
        }

        private async Task<DetallesPeriodoEscolarDTO> obtenerPeriodoDTOAsync(int idPeriodo)
        {
            var periodo = await _periodoEscolarRepository.ObtenerPorIdAsync(idPeriodo);
            var periodoDTO = PeriodoEscolarMapper.ToDTO(periodo);
            return periodoDTO;
        }
    }
}
