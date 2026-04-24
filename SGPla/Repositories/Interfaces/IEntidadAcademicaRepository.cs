using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface IEntidadAcademicaRepository
    {
        Task<List<EntidadAcademica>> ObtenerTodosAsync();
        Task<List<EntidadAcademica>> ObtenerDiezAsync(int indiceInicial);
        Task<List<EntidadAcademica>> ObtenerPorFiltroAsync(string? region, int? idAreaAcademica, string? nombre);
        Task<EntidadAcademica?> ObtenerPorIdAsync(int idEntidadAcademica);
        Task<List<EntidadAcademica>> ObtenerPorIdAreaAcademicaAsync(int idAreaAcademica);
        Task<bool> ExistePorIdAsync(int idEntidadAcademica);
        Task<bool> ExistePorClaveAsync(string clave);
        Task<bool> ExistePorClaveAsync(string clave, int idEntidadAcademica);
        Task<EntidadAcademica> CrearAsync(EntidadAcademica entidadAcademica);
        Task ActualizarAsync(EntidadAcademica entidadAcademica);
        Task EliminarAsync(EntidadAcademica entidadAcademica);

        Task<List<EntidadAcademica>> ObtenerOpcionesAsync();

    }
}
