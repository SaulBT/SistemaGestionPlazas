using SGPla.Models;
using SGPla.Models.DTOs.Aviso;

namespace SGPla.Services.Interfaces
{
    public interface IAvisoService
    {
        Task<(List<ListaAvisosDTO> items, int total)> ObtenerTodosAvisosAsync(FiltroAvisosDTO filtroDTO);
        Task<DatosAvisoDTO> ObtenerAvisoPorIDAsync(int idAviso);
        Task CrearAviso(CrearAvisoDTO aviso);
        Task EliminarAvisoPorId(int idAviso);
        Task ActualizarAvisoPorId(EditarAvisoDTO aviso);

        //DGAA
        Task RevisarAvisoAsync(RevisionDTO revisionDTO);
        Task ArchivarAvisoAsync(int idAviso);
    }
}
