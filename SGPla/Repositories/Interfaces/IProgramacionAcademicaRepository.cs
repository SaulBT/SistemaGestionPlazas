using SGPla.Models;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.ProgramacionAcademica;
namespace SGPla.Repositories.Interfaces
{
    public interface IProgramacionAcademicaRepository
    {


        Task<List<ResumenOfertaProgramacionAcademicaDTO>> ObtenerResumenPorProgramaPeriodoAsync(BuscarProgramacionAcademicaDTO? filtro);

        Task<int?> ObtenerPeriodoMasRecienteConProgramacionAsync(bool incluirPlanea);

        Task<List<OfertaDTO>> ObtenerOfertasExperienciasEducativasAsync(int idEntidadAcademica, int idProgramaEducativo, int idPeriodo, string? busqueda = null);

        Task<OfertaDTO?> ObtenerOfertaPorId(int idOferta);

        Task<bool> EditarOfertaAsync(int idOferta, OfertaDTO ofertaDTO);

        Task<List<Log>> ObtenerLogsPorOfertaAsync(int idOferta);

        Task EliminarOfertaAsync(int idOferta);

        Task CambiarInclusionOfertaAsync(int idOferta, bool incluir);

        Task CambiarAVacanteAsync(int idOferta, string justificacion);

        Task AsignarDocenteAsync(int idOferta, int idDocente);
    }

}
