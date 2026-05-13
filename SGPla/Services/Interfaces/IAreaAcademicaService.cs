using SGPla.Models.DTOs.AreaAcademica;

namespace SGPla.Services.Interfaces
{
    public interface IAreaAcademicaService
    {
        Task<int> CrearAsync(CrearAreaAcademicaDTO dto);
        Task<List<ListaAreaAcademicaDTO>> ObtenerTodasAsync();
        Task<List<ListaAreaAcademicaDTO>> ObtenerPorNombreAsync(string nombre);
        Task<DatosAreaAcademicaDTO> ObtenerPorIdAsync(int id);
        Task EditarAsync(DatosAreaAcademicaDTO dto);
        Task EliminarAsync(int id);
        Task<(List<ListaAreaAcademicaDTO> Items, int TotalCount)> BuscarPorFiltroPaginadoAsync(string busqueda, int pagina, int cantidad);
        //Task<(List<Detalles>)>
    }
}
