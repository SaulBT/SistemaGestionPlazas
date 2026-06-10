using SGPla.Models;
using SGPla.Models.DTOs.IntegranteCt;

namespace SGPla.Repositories.Interfaces
{
    public interface IIntegranteCtRepository
    {
        Task RegistrarAsync(IntegranteCt integrante);
        Task EditarAsync(IntegranteCt integrante);
        Task<List<IntegranteCt>> ObtenerTodosAsync(int idEntidadAcademica);
        Task<List<IntegranteCt>> ObtenerPorPaginaAsync(int idEntidadAcademica, int pagina, int cantidad);
        Task<List<IntegranteCt>> ObtenerPorNombrePaginaAsync(int idEntidadAcademica, string nombre, int pagina, int cantidad);
        Task<IntegranteCt?> ObtenerPorIdAsync(int idIntegranteCt);
        Task EliminarAsync(IntegranteCt integrante);
        Task<bool> ExistePorIdAsync(int idIntegranteCt);
        Task<bool> ExistePorNombre(string nombre, int idEntidadAcademica);
        Task<int> ContarAsync(int idEntidadAcademica);
        Task<int> ContarAsync(int idEntidadAcademica, string busqueda);
    }
}
