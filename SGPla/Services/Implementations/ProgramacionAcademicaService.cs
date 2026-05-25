using SGPla.Models.DTOs.Oferta;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Services.Implementations
{
    public class ProgramacionAcademicaService : IProgramacionAcademicaService
    {
        public readonly IProgramacionAcademicaValidator _programacionAcademicaValidator;

        public ProgramacionAcademicaService(IProgramacionAcademicaValidator programacionAcademicaValidator)
        {
            _programacionAcademicaValidator = programacionAcademicaValidator;
        }

        public async Task<List<OfertaDTO>> ProcesarArchivoAsync(IFormFile archivo)
        {
            using var ms = new MemoryStream();
            await archivo.CopyToAsync(ms);
            ms.Position = 0;

            var ofertas = DescargasParser.Parse(ms, archivo.FileName);

            await _programacionAcademicaValidator.ValidarDocentes(ofertas);
            await _programacionAcademicaValidator.ValidarProgramas(ofertas);
            await _programacionAcademicaValidator.ValidarExperiencias(ofertas);

            return ofertas;
        }
    }
}
