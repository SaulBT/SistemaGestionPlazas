using ExcelDataReader;
using HtmlAgilityPack;
using SGPla.Models;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.ProgramacionAcademica;
using System.Text;
using System.Text.RegularExpressions;



public interface IProgramacionAcademicaService
{
    Task<List<OfertaDTO>> ProcesarArchivoAsync(IFormFile archivo);

    Task<bool> GuardarOfertasAsync(List<OfertaDTO> ofertas, int idPeriodo, int idArticulo);

    Task<List<CargaConOfertaDTO>> ProcesarCargasAsync(
    IFormFile archivoCarga,
    List<OfertaDTO> ofertasEnSesion);

    Task<List<EntidadAcademica>> ObtenerOpcionesEntidadAcademicaAsync(string region);
}

