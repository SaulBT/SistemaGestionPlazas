

using SGPla.Models.DTOs.PeriodoEscolar;

namespace SGPla.Services.Interfaces
{
    public interface IPeriodoEscolarService
    {
        Task<List<DetallesPeriodoEscolarDTO>> ObtenerTodosAsync();
        Task<DetallesPeriodoEscolarDTO?> ObtenerPorIdAsync(int id);

        Task<DetallesPeriodoEscolarDTO> CrearAsync(CrearPeriodoEscolarDTO periodoEscolarDTO);

        Task<DetallesPeriodoEscolarDTO> EditarAsync(EditarPeriodoEscolarDTO periodoEscolarDTO);

        Task<bool> EliminarAsync(int id);

        Task<List<DetallesPeriodoEscolarDTO>> BuscarPorFiltroAsync(BuscarPeriodoEscolarDTO   filtro);

        Task<(List<DetallesPeriodoEscolarDTO> Items, int TotalCount)> BuscarPorFiltroPaginadoConTotalAsync(BuscarPeriodoEscolarDTO filtro);


    }
}
