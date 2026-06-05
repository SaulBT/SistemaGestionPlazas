using SGPla.Models;
using SGPla.Models.DTOs.Oferta;
namespace SGPla.Repositories.Interfaces
{
    public interface IProgramacionAcademicaRepository
    {
        Task<List<string>> ObtenerRelacionesValidasAsync(List<OfertaDTO> ofertas);

        Task<bool> GuardarOfertas(List<Oferta> ofertas);
    }
}
