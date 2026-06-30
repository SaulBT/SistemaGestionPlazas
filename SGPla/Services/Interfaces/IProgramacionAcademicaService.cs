using ExcelDataReader;
using HtmlAgilityPack;
using SGPla.Models;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.ProgramacionAcademica;
using System.Text;
using System.Text.RegularExpressions;



public interface IProgramacionAcademicaService
{
    Task<List<OfertaDTO>> ProcesarArchivoOfertasAsync(IFormFile archivo, TipoArchivoOferta tipoArchivo);

    Task<bool> GuardarOfertasAsync(List<OfertaDTO> ofertas, List<CargaConOfertaDTO> cargas);

    Task<List<CargaConOfertaDTO>> ProcesarCargasAsync(
    IFormFile archivoCarga);

    Task<List<EntidadAcademica>> ObtenerOpcionesEntidadAcademicaAsync(string region);

    Task<List<ProgramaEducativo>> ObtenerOpcionesProgramaEducativoAsync(int idEntidadAcademica);

    Task<List<ResumenOfertaProgramacionAcademicaDTO>> ObtenerResumenPorProgramaPeriodoAsync(BuscarProgramacionAcademicaDTO? filtro);

    Task<List<OfertaDTO>> ObtenerOfertasGuardadasAsync(int idEntidadAcademica, int idProgramaEducativo, int idPeriodo);


}

