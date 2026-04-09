using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface IAreaAcademicaRepository
    {
        Task<List<AreaAcademica>> ObtenerTodosAsync();
        Task<AreaAcademica?> ObtenerPorIdAsync(int idAreaAcademica);
        Task<bool> ExistePorIdAsync(int idAreaAcademica);
        Task<AreaAcademica> CrearAsync(AreaAcademica areaAcademica);
        Task ActualizarAsync(AreaAcademica areaAcademica);
    }
}
