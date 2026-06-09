using SGPla.Models.DTOs.IntegranteCt;

namespace SGPla.Services.Interfaces
{
    public interface IIntegranteCtService
    {
        Task RegistrarIntegranteAsync(RegistrarIntegranteCtDto integranteDto);
        Task EditarIntegranteAsync(DatosIntegranteCtDto integranteDto);
        Task<DatosIntegranteCtDto> ObtenerIntegrantePorIdAsync(int idIntegranteCt);
        Task<List<DatosIntegranteCtDto>> ObtenerTodosIntegrantesAsync(int idEntidadAcademica);
        Task<List<DatosIntegranteCtDto>> ObtenerTodosIntegrantesPorPaginaAsync(BusquedaIntegranteCtDto busquedaDto);
        Task EliminarIntegranteAsync(int idIntegranteCt);
    }
}
