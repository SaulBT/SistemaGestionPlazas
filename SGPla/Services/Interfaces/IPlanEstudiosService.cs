using SGPla.Models.DTOs.PlanEstudios;

namespace SGPla.Services.Interfaces
{
    public interface IPlanEstudiosService
    {
        List<DatosExperienciaEducativaDTO> ProcesarArchivo(ArchivoPlanEstudiosDTO archivoPlanEstudiosDTO);
        Task<int> AgregarAsync(CrearPlanEstudiosDTO crearPlanEstudiosDTO);
        Task<List<ListaPlanEstudiosDTO>> ObtenerTodosAsync();
        Task<List<ListaPlanEstudiosDTO>> ObtenerDiezAsync(int indice);
        Task<List<ListaPlanEstudiosDTO>> ObtenerPorFiltroAsync(FiltroPlanEstudiosDTO filtroPlanEstudiosDTO, int indice);
        Task<DatosPlanEstudiosDTO> ObtenerPorIdAsync(int idPlanEstudios);
        Task EditarAsync(EditarPlanEstudiosDTO editarPlanEstudiosDTO);
        Task EliminarAsync(int idPlanEstudios);
    }
}
