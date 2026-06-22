using SGPla.Models;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.ProgramacionAcademica;
namespace SGPla.Repositories.Interfaces
{
    public interface IProgramacionAcademicaRepository
    {
        Task<List<string>> ObtenerRelacionesValidasAsync(List<OfertaDTO> ofertas);

        Task GuardarOfertas(List<Oferta> ofertas);

        Task<List<ResumenOfertaProgramacionAcademicaDTO>> ObtenerResumenPorProgramaPeriodoAsync(BuscarProgramacionAcademicaDTO? filtro);

        Task<List<OfertaDTO>> ObtenerOfertasGuardadasAsync(int idEntidadAcademica, int idProgramaEducativo, int idPeriodo);


    }
}
