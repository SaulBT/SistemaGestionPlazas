using SGPla.Mappers;
using SGPla.Models;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.ProgramacionAcademica;
using SGPla.Parsers;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Interfaces;

public enum TipoArchivoOferta
{
    Vacantes,
    Descargas
}

namespace SGPla.Services.Implementations
{
    public class ProgramacionAcademicaService : IProgramacionAcademicaService
    {
        public readonly IProgramacionAcademicaValidator _programacionAcademicaValidator;
        public readonly IProgramacionAcademicaRepository _programacionAcademicaRepository;
        public readonly IDocenteRepository _docenteRepository;
        public readonly IExperienciaEducativaRepository _experienciaRepository; 
        public readonly IEntidadAcademicaRepository _entidadAcademicaRepository;
        public readonly IProgramaEducativoRepository _programaEducativoRepository;


        public ProgramacionAcademicaService(IProgramacionAcademicaValidator programacionAcademicaValidator, IProgramacionAcademicaRepository programacionAcademicaRepository, IDocenteRepository docenteRepository, IExperienciaEducativaRepository experienciaRepository, IEntidadAcademicaRepository entidadAcademicaRepository, IProgramaEducativoRepository programaEducativoRepository )
        {
            _programacionAcademicaValidator = programacionAcademicaValidator;
            _programacionAcademicaRepository = programacionAcademicaRepository;
            _docenteRepository = docenteRepository;
            _experienciaRepository = experienciaRepository;
            _entidadAcademicaRepository = entidadAcademicaRepository;
            _programaEducativoRepository = programaEducativoRepository;
        }

        public async Task<List<OfertaDTO>> ProcesarArchivoOfertasAsync(IFormFile archivo, TipoArchivoOferta tipoArchivo)
        {
            using var ms = new MemoryStream();
            await archivo.CopyToAsync(ms);
            ms.Position = 0;
            string tipoContratacion = tipoArchivo == TipoArchivoOferta.Vacantes ? "IOD" : "IPP";

            var ofertas = DescargasParser.Parse(ms, archivo.FileName);

           ofertas.ForEach(o => o.TC = tipoContratacion);



            //await _programacionAcademicaValidator.ValidarProgramas(ofertas);
            //await _programacionAcademicaValidator.ValidarExperiencias(ofertas);
            //await _programacionAcademicaValidator.ValidarDocentes(ofertas);


            return ofertas;
        }

        public async Task<bool> GuardarOfertasAsync(List<OfertaDTO> ofertas)
        {
            var numerosPersonal = ofertas
                .Select(o => o.NP)
                .Where(np => !string.IsNullOrWhiteSpace(np))
                .Distinct()
                .ToList();

            var nombresExperiencia = ofertas
                .Select(o => o.ExperienciaEducativa)
                .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
                .Distinct()
                .ToList();

            var docentes = await _docenteRepository
                .ObtenerIdsPorNumeroPersonalAsync(numerosPersonal);

            var experiencias = await _experienciaRepository
                .ObtenerIdsPorNombreAsync(nombresExperiencia);

            var nombresProgramas = ofertas
     .Where(o => !string.IsNullOrWhiteSpace(o.Programa))
     .Select(o => o.Programa)
     .Distinct()
     .ToList();

            var idsProgramas = await _programaEducativoRepository
                .ObtenerIdsProgramasAsync(nombresProgramas);

           
            var ofertasModel = new List<Oferta>();

            foreach (var dto in ofertas)
            {
                dto.IdProgramaEducativo = idsProgramas[dto.Programa];
                var oferta = OfertaMapper.ToModel(dto);

                if (!string.IsNullOrWhiteSpace(dto.NP))
                {
                    if (docentes.TryGetValue(dto.NP, out var idDocente))
                    {
                        oferta.IdDocente = idDocente;
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"No existe un docente con NP '{dto.NP}'.");
                    }
                }

                if (experiencias.TryGetValue(dto.ExperienciaEducativa, out var idExperiencia))
                {
                    oferta.IdExperienciaEducativa = idExperiencia;
                }
                else
                {
                    throw new ArgumentException(
                        $"No existe una experiencia con nombre '{dto.ExperienciaEducativa}'.");
                }

                ofertasModel.Add(oferta);
            }

            await _programacionAcademicaRepository
                .GuardarOfertas(ofertasModel);

            return true;
        }
        
        public async Task<List<CargaConOfertaDTO>> ProcesarCargasAsync(
            IFormFile archivoCarga,
            List<OfertaDTO> ofertasEnSesion)
        {
            using var stream = archivoCarga.OpenReadStream();
            var cargas = CargasParser.Parse(stream);

            var ofertasPorNrc = ofertasEnSesion
                .GroupBy(o => o.NRC?.Trim() ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.First(),
                              StringComparer.OrdinalIgnoreCase);

            var resultado = new List<CargaConOfertaDTO>();

            foreach (var docente in cargas.Docentes)
            {
                foreach (var experiencia in docente.Experiencias)
                {
                    ofertasPorNrc.TryGetValue(experiencia.Nrc, out var oferta);

                    resultado.Add(new CargaConOfertaDTO
                    {
                        NumeroPersonal = docente.NumeroPersonal,
                        NombreDocente = docente.Nombre,
                        Plaza = experiencia.Plaza,
                        Categoria = experiencia.Categoria,
                        TipoContratacion = experiencia.TipoContratacion,
                        Nrc = experiencia.Nrc,
                        ExperienciaEducativa = experiencia.ExperienciaEducativa,
                        HorasContacto = experiencia.HorasContacto,
                        HorasPago = experiencia.HorasPago,
                        MotivoRh = experiencia.MotivoRh,
                        IndActDocente = experiencia.IndActDocente,
                        Imparte = experiencia.Imparte,
                        NrcEncontrado = oferta is not null,
                        Programa = oferta?.Programa,
                        NpOferta = oferta?.NP,
                        DocenteOferta = oferta?.NombreDocente,
                    });
                }
            }

            return resultado;
        }

        public async Task<List<EntidadAcademica>> ObtenerOpcionesEntidadAcademicaAsync(string region)
        {
            return await _entidadAcademicaRepository.ObtenerOpcionesAsync(region);
        }

    }
}
