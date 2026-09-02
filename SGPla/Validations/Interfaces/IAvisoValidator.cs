using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Aviso;

namespace SGPla.Validations.Interfaces
{
    public interface IAvisoValidator
    {
        Task ValidarIdAsync(int idAviso);
        Task ValidarEnviarARevisionAsync(RevisionDTO revisionDTO);
        Task ValidarArchivoAsync(CargarArchivoDTO cargarArchivoDTO);
        Task ValidarPublicacionAsync(int idAaviso, string url);
        Task ValidarCrearAviso(CrearAvisoDTO dto);
    }
}
