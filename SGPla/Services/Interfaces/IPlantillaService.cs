using SGPla.Models.DTOs.Plantillas;

namespace SGPla.Services.Interfaces
{
    public interface IPlantillaService
    {
        Task<int> GenerarAvisoAsync(PlantillaAvisoDTO plantillaAvisoDTO);
        Task<string> RenderizarAvisoAsync(PlantillaAvisoDTO plantillaAvisoDTO);
    }
}
