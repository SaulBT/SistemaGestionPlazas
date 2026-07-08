using SGPla.Models;
using SGPla.Models.DTOs.Aviso;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.PlanEstudios;

namespace SGPla.Services.Interfaces
{
    public interface IAvisoService
    {
        //Propios de Aviso
        Task<(List<ListaAvisosDTO> items, int total)> ObtenerTodosAvisosAsync(FiltroAvisosDTO filtroDTO);
        Task<DatosAvisoDTO> ObtenerAvisoPorIDAsync(int idAviso);
        Task CrearAviso(CrearAvisoDTO aviso);
        Task EliminarAvisoPorId(int idAviso);
        Task ActualizarAvisoPorId(EditarAvisoDTO aviso);

        //DGAA
        Task RevisarAvisoAsync(RevisionDTO revisionDTO);
        Task ArchivarAvisoAsync(int idAviso);

        //Métodos para obtener datos necesarios
        Task<List<OfertaPlanEstudiosAvisoDTO>> ObtenerPlanesConOfertasAviso(int idEntidadAcademica, int idPeriodo, int idArticulo);
    }
}
