using SGPla.Models;
using SGPla.Models.DTOs.Archivo;
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
        Task EditarComentariosRevisionAsync(int idAviso, string comentarios);
        int ObtenerIdArchivoVigente(DatosAvisoDTO aviso);
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

        //Métodos para obtener datos necesarios
        Task<List<OfertaPlanEstudiosAvisoDTO>> ObtenerPlanesConOfertasAviso(int idEntidadAcademica, int idPeriodo, int idArticulo);
    }
}
