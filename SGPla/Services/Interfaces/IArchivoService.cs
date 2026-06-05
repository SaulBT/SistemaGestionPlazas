using SGPla.Models.DTOs.Archivo;

namespace SGPla.Services.Interfaces
{
    public interface IArchivoService
    {
        Task<DatosArchivoGuardadoDTO> GuardarAsync(string archivo, string nombreOriginal, string carpeta);
        Task<(string nombre, string ruta)> GuardarTemporalmenteAsync(IFormFile archivo);
        Task<ArchivoDescargadoDTO> DescargarAsync(int idArchivo);
        Task EliminarAsync(string rutaRelativa);
    }
}
