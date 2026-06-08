using SGPla.Mappers;
using SGPla.Models;
using SGPla.Models.DTOs.Cargas;
using SGPla.Models.DTOs.Oferta;
using SGPla.Parsers;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Services.Implementations
{
    public class ProgramacionAcademicaService : IProgramacionAcademicaService
    {
        public readonly IProgramacionAcademicaValidator _programacionAcademicaValidator;
        public readonly IProgramacionAcademicaRepository _programacionAcademicaRepository;
        public readonly IDocenteRepository _docenteRepository;
        public readonly IExperienciaEducativaRepository _experienciaRepository; 

        public ProgramacionAcademicaService(IProgramacionAcademicaValidator programacionAcademicaValidator, IProgramacionAcademicaRepository programacionAcademicaRepository, IDocenteRepository docenteRepository, IExperienciaEducativaRepository experienciaRepository)
        {
            _programacionAcademicaValidator = programacionAcademicaValidator;
            _programacionAcademicaRepository = programacionAcademicaRepository;
            _docenteRepository = docenteRepository;
            _experienciaRepository = experienciaRepository;
        }

        public async Task<List<OfertaDTO>> ProcesarArchivoAsync(IFormFile archivo)
        {
            using var ms = new MemoryStream();
            await archivo.CopyToAsync(ms);
            ms.Position = 0;

            var ofertas = DescargasParser.Parse(ms, archivo.FileName);

            //await _programacionAcademicaValidator.ValidarDocentes(ofertas);
            //await _programacionAcademicaValidator.ValidarProgramas(ofertas);
            //await _programacionAcademicaValidator.ValidarExperiencias(ofertas);

            return ofertas;
        }

        public async Task<bool> GuardarOfertasAsync(List<OfertaDTO> ofertas, int idPeriodo)
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

            var ofertasModel = new List<Oferta>();
            foreach (var dto in ofertas)
            {
                var oferta = OfertaMapper.ToModel(dto);
                oferta.IdPeriodo = idPeriodo;

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

      

        // Implementación
        public async Task<List<CargaConOfertaDTO>> ProcesarCargasAsync(
            IFormFile archivoCarga,
            List<OfertaDTO> ofertasEnSesion)
        {
            using var stream = archivoCarga.OpenReadStream();
            var cargas = CargasParser.Parse(stream);

            // Índice rápido NRC → oferta
            var ofertasPorNrc = ofertasEnSesion
                .GroupBy(o => o.NRC?.Trim() ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.First(),
                              StringComparer.OrdinalIgnoreCase);

            var resultado = new List<CargaConOfertaDTO>();

            foreach (var docente in cargas.Docentes)
            {
                foreach (var materia in docente.Materias)
                {
                    ofertasPorNrc.TryGetValue(materia.Nrc, out var oferta);

                    resultado.Add(new CargaConOfertaDTO
                    {
                        NumeroPersonal = docente.NumeroPersonal,
                        NombreDocente = docente.Nombre,
                        Plaza = docente.Plaza,
                        Categoria = docente.Categoria,
                        TipoContratacion = docente.TipoContratacion,
                        Nrc = materia.Nrc,
                        ExperienciaEducativa = materia.ExperienciaEducativa,
                        HorasContacto = materia.HorasContacto,
                        HorasPago = materia.HorasPago,
                        MotivoRh = materia.MotivoRh,
                        IndActDocente = materia.IndActDocente,
                        Imparte = materia.Imparte,
                        // Cruce
                        NrcEncontrado = oferta is not null,
                        Programa = oferta?.Programa,
                        NpOferta = oferta?.NP,
                        DocenteOferta = oferta?.NombreDocente,
                    });
                }
            }

            foreach (var docente in cargas.Docentes)
            {
                foreach (var materia in docente.Materias)
                {
                    if (ofertasPorNrc.ContainsKey(materia.Nrc))
                    {
                        Console.WriteLine($"MATCH: {materia.Nrc}");
                    }
                }
            }

            var nrcOferta = ofertasPorNrc.Keys.OrderBy(x => x).ToList();

            var nrcEncontrados = cargas.Docentes
                .SelectMany(d => d.Materias)
                .Where(m => ofertasPorNrc.ContainsKey(m.Nrc))
                .Select(m => m.Nrc)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            Console.WriteLine($"Oferta: {nrcOferta.Count}");
            Console.WriteLine($"Encontrados: {nrcEncontrados.Count}");

            var faltantes = nrcOferta.Except(nrcEncontrados);

            foreach (var nrc in faltantes)
            {
                Console.WriteLine($"FALTA: {nrc}");
            }

            return resultado;
        }

    }
}
