
using SGPla.Models;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.ProgramacionAcademica;

public interface IProgramacionAcademicaService
{
    Task<List<OfertaDTO>> ProcesarArchivoOfertasAsync(IFormFile archivo, TipoArchivoOferta tipoArchivo);

    Task<bool> GuardarOfertasyCargasAsync(List<OfertaDTO> ofertas, List<CargaConOfertaDTO> cargas);

    Task<List<CargaConOfertaDTO>> ProcesarCargasAsync(
    IFormFile archivoCarga);

    Task<List<EntidadAcademica>> ObtenerOpcionesEntidadAcademicaAsync(string region);

    Task<List<ProgramaEducativo>> ObtenerOpcionesProgramaEducativoAsync(int idEntidadAcademica);

    Task<List<ResumenOfertaProgramacionAcademicaDTO>> ObtenerResumenPorProgramaPeriodoAsync(BuscarProgramacionAcademicaDTO? filtro);

    Task<List<OfertaDTO>> ObtenerOfertasGuardadasAsync(int idEntidadAcademica, int idProgramaEducativo, int idPeriodo);

    Task<OfertaDTO?> ObtenerOfertaPorId(int idOferta);

    Task<bool> EditarOfertaAsync (int idOferta, OfertaDTO ofertaDTO);


    Task<List<LogDTO>> ObtenerHistorialPorIdOferta(int idOferta);

    Task<List<Log>> EliminarOfertaAsync(int idOferta);


    Task CambiarInclusionOfertaAsync(int idOferta, bool incluir);

    Task CambiarAVacanteAsync(int idOferta, string justificacion);

}

