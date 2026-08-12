using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface IArchivoRepository
    {
        Task<Archivo> CrearAsync(Archivo archivo);
        Task<Archivo?> ObtenerPorIdAsync(int idArchivo);
        Task ActualizarAsync(Archivo archivo);
        Task EliminarAsync(Archivo archivo);

    }
}