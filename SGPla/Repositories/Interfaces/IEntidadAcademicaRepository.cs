using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface IEntidadAcademicaRepository
    {
        Task<List<EntidadAcademica>> ObtenerTodosAsync();
        Task<EntidadAcademica?> ObtenerPorIdAsync(int idEntidadAcademica);
        Task<List<EntidadAcademica>> ObtenerPorIdAreaAcademicaAsync(int idAreaAcademica);
        Task<bool> ExistePorIdAsync(int idEntidadAcademica);
        Task<EntidadAcademica> CrearAsync(EntidadAcademica entidadAcademica);
        Task ActualizarAsync(EntidadAcademica entidadAcademica);
    }
}
