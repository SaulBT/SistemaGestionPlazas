using SGPla.Models.DTOs.PlanEstudios;

namespace SGPla.Services.Interfaces
{
    public interface IPlanEstudiosService
    {
        List<DatosExperienciaEducativaDTO> ProcesarArchivo(ArchivoPlanEstudiosDTO archivoPlanEstudiosDTO);
        Task<int> AgregarAsync(CrearPlanEstudiosDTO crearPlanEstudiosDTO);
        Task<List<ListaPlanEstudiosDTO>> ObtenerTodosAsync();
        Task<(List<ListaPlanEstudiosDTO> items, int cantidad)> ObtenerPorPaginaAsync(int pagina, int cantidad);
        Task<(List<ListaPlanEstudiosDTO> items, int cantidad)> ObtenerPorFiltroAsync(FiltroPlanEstudiosDTO filtroPlanEstudiosDTO, int pagina, int cantidad);
        Task<DatosPlanEstudiosDTO> ObtenerPorIdAsync(int idPlanEstudios);
        Task EditarAsync(EditarPlanEstudiosDTO editarPlanEstudiosDTO);
        Task EliminarAsync(int idPlanEstudios);
    }
}