using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface IOfertaRepository
    {
        Task<List<Oferta>> ObtenerPorAvisoAsync(int idAviso);
    }
}
