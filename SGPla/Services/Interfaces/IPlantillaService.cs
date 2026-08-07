using SGPla.Models.DTOs.Plantillas;

namespace SGPla.Services.Interfaces
{
    public interface IPlantillaService
    {
        Task GenerarAvisoAsync(PlantillaAvisoDTO plantillaAvisoDTO);
    }
}
