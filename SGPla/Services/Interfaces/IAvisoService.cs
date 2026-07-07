using SGPla.Models;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Aviso;

namespace SGPla.Services.Interfaces
{
    public interface IAvisoService
    {
        Task<(List<ListaAvisosDTO> items, int total)> ObtenerTodosAvisosAsync(FiltroAvisosDTO filtroDTO);
        Task<DatosAvisoDTO> ObtenerAvisoPorIDAsync(int idAviso);
        Task ArchivarAvisoAsync(int idAviso);
        Task DesarchivarAvisoAsync(int idAviso);
        Task<bool> VerificarEstadoAvisoAsync(int idAviso, string estado);
        
        //EA
        Task CrearAviso(CrearAvisoDTO aviso);
        Task EliminarAvisoPorId(int idAviso);
        Task ActualizarAvisoPorId(EditarAvisoDTO aviso);
        Task EnviarARevisionAsync(RevisionDTO revisionDTO);
        Task FirmarAvisoAsync(int idAviso, CargarArchivoDTO archivoDTO);
        Task<string> VerComentariosAsync(int idAviso);
        Task PublicarAvisoAsync(int idAviso, string url);

        //DGAA
        Task RevisarAvisoAsync(RevisionDTO revisionDTO);
    }
}
