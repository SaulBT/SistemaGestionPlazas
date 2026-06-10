using HtmlAgilityPack;
using SGPla.Models.DTOs.Oferta;
using System.Text;
using ExcelDataReader;
using System.Text.RegularExpressions;
using SGPla.Models.DTOs.ProgramacionAcademica;



public interface IProgramacionAcademicaService
{
    Task<List<OfertaDTO>> ProcesarArchivoAsync(IFormFile archivo);

    Task<bool> GuardarOfertasAsync(List<OfertaDTO> ofertas, int idPeriodo, int idArticulo);

    Task<List<CargaConOfertaDTO>> ProcesarCargasAsync(
    IFormFile archivoCarga,
    List<OfertaDTO> ofertasEnSesion);
}

