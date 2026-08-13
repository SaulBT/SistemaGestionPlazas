using SGPla.Models;
using SGPla.Models.DTOs.PlanEstudios;

namespace SGPla.Repositories.Interfaces
{
    public interface IOfertaRepository
    {
        Task<List<Oferta>> ObtenerPorAvisoAsync(int idAviso);
        Task<List<OfertaPlanEstudiosAvisoDTO>> ObtenerPlanesEstudioCrearAviso(int idEntidadAcademica, int idPeriodo, int idArticulo);
        Task<Oferta?> ObtenerPorIdAsync(int id);
    }
}
