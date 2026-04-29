using SGPla.Models;
using SGPla.Models.DTOs.PeriodoEscolar;

namespace SGPla.Repositories.Interfaces
{
    public interface IPeriodoEscolarRepository
    {
        Task<List<Periodo>> ObtenerTodosAsync();
        Task<Periodo?> ObtenerPorIdAsync(int idPeriodoEscolar);
        Task<List<Periodo>> ObtenerPorFiltroAsync(BuscarPeriodoEscolarDTO filtro);
        Task<Periodo?> ExisteAsync(Periodo periodoEscolar);
        Task<Periodo> CrearAsync(Periodo periodoEscolar);
        Task<Periodo?> ActualizarAsync(Periodo periodoEscolar);

        Task<bool> EliminarAsync(int id);
    }
}
