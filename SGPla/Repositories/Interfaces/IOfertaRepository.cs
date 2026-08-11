using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.PlanEstudios;

namespace SGPla.Repositories.Interfaces
{
    public interface IOfertaRepository
    {
        Task<List<Oferta>> ObtenerPorAvisoAsync(int idAviso);
        Task<List<OfertaPlanEstudiosAvisoDTO>> ObtenerPlanesEstudioCrearAviso(int idEntidadAcademica, int idPeriodo, int idArticulo);
    }
}
