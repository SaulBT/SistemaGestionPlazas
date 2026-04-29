using SGPla.Models.DTOs.Archivo;

namespace SGPla.Services.Interfaces
{
    public interface IArchivoService
    {
        Task<DatosArchivoGuardadoDTO> GuardarAsync(Stream archivo, string nombreOriginal, string carpeta);
        Task<ArchivoDescargadoDTO> DescargarAsync(int idArchivo);
        Task EliminarAsync(string rutaRelativa);
    }
}
