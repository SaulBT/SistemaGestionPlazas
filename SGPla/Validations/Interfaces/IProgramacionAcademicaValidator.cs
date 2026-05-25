using SGPla.Models.DTOs.Oferta;

namespace SGPla.Validations.Interfaces
{
    public interface IProgramacionAcademicaValidator
    {
        Task<bool> ValidarDocentes(List<OfertaDTO> ofertas);

        Task<bool> ValidarProgramas(List<OfertaDTO> ofertas);

        Task<bool> ValidarExperiencias(List<OfertaDTO> ofertas);
    }
}
