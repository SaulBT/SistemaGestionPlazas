using SGPla.Models.DTOs.PlanEstudios;

namespace SGPla.Validations.Interfaces
{
    public interface IPlanEstudiosValidator
    {
        void ValidarArchivo(ArchivoPlanEstudiosDTO archivoPlanEstudiosDTO);
        void ValidarIndice(int indice);
        Task ValidarCreacionAsync(CrearPlanEstudiosDTO crearPlanEstudiosDTO);
        Task ValidarEdicionAsync(EditarPlanEstudiosDTO editarPlanEstudiosDTO);
        Task ValidarIdAsync(int idPlanEstudios);
    }
}
