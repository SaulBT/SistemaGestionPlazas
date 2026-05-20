using SGPla.Models.DTOs.EntidadAcademica;

namespace SGPla.Services.Interfaces
{
    public interface IEntidadAcademicaService
    {
        Task<int> CrearAsync(CrearEntidadAcademicaDTO dto);
        Task<List<ListaEntidadAcademicaDTO>> ObtenerCatalogoAsync(FiltroEntidadAcademicaDTO filtro);
        Task<List<ListaEntidadAcademicaDTO>> ObtenerListaAsync(int indice);
        Task<List<ListaEntidadAcademicaDTO>> ObtenerPorFiltroAsync(FiltroEntidadAcademicaDTO filtro, int indice);
        Task<DatosEntidadAcademicaDTO> ObtenerPorIdAsync(int id);
        Task EditarAsync(DatosEntidadAcademicaDTO dto);
        Task EliminarAsync(int id);

        Task<(List<ListaEntidadAcademicaDTO> Items, int TotalCount)> BuscarPorFiltroPaginadoAsync(FiltroEntidadAcademicaDTO filtro);

    }
}
