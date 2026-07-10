using SGPla.Mappers;
using SGPla.Models;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.ProgramacionAcademica;
using SGPla.Models.DTOs.ProgramaEducativo;
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

        public async Task<bool> GuardarOfertasyCargasAsync(List<OfertaDTO> ofertas, List<CargaConOfertaDTO> cargas)
        {
            await _programacionAcademicaValidator.ValidarProgramas(ofertas);
            await _programacionAcademicaValidator.ValidarExperiencias(ofertas);
            await _programacionAcademicaValidator.ValidarDocentes(ofertas);
            await _programacionAcademicaValidator.ValidarArticulo(ofertas);

            var numerosPersonalOfertas = ofertas
                .Select(o => o.NP)
                .Where(np => !string.IsNullOrWhiteSpace(np))
                .Distinct()
                .ToList();

            var nombresExperiencia = ofertas
                .Select(o => o.ExperienciaEducativa)
                .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
                .Distinct()
                .ToList();

            var docentesOfertas = await _docenteRepository
                .ObtenerIdsPorNumeroPersonalAsync(numerosPersonalOfertas);

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


            var numerosPersonalCargas = cargas
                .Select(c => c.NumeroPersonal)
                .Where(np => !string.IsNullOrWhiteSpace(np))
                .Distinct()
                .ToList();

            var docentesCargas = await _docenteRepository
                .ObtenerIdsPorNumeroPersonalAsync(numerosPersonalCargas);

            var cargasModel = new List<CargaAcademica>();

            foreach (var dto in ofertas)
            {
                var clavePrograma = ObtenerClavePrograma(dto.Programa);

                if (!idsProgramas.TryGetValue(clavePrograma, out var idPrograma))
                {
                    throw new ArgumentException(
                        $"No existe un programa con clave '{clavePrograma}'.");
                }

                dto.IdProgramaEducativo = idPrograma;

                var oferta = OfertaMapper.ToModel(dto);

                if (!string.IsNullOrWhiteSpace(dto.NP))
                {
                    if (docentesOfertas.TryGetValue(dto.NP, out var idDocente))
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

            foreach (var carga in cargas)
            {
                if (!string.IsNullOrWhiteSpace(carga.Programa))
                {
                    var clavePrograma = ObtenerClavePrograma(carga.Programa);
                    if (!idsProgramas.TryGetValue(clavePrograma, out var idPrograma))
                    {
                        throw new ArgumentException(
                            $"No existe un programa con clave '{clavePrograma}'.");
                    }

                    if (!docentesCargas.TryGetValue(carga.NumeroPersonal, out var idDocente))
                    {
                        throw new ArgumentException(
                            $"No existe un docente con NP '{carga.NumeroPersonal}'.");
                    }
                    carga.idDocente = idDocente;

                    bool encontrada = experiencias.TryGetValue(carga.ExperienciaEducativa, out var idExperiencia);

                    if (!encontrada && !string.IsNullOrWhiteSpace(carga.Nrc))
                    {
                        throw new ArgumentException(
                            $"No existe una experiencia con nombre '{carga.ExperienciaEducativa}'.");
                    }

                    carga.idExperienciaEducativa = encontrada ? idExperiencia : null;

                    carga.idPeriodo = ofertasModel
                        .FirstOrDefault(o => o.IdProgramaEducativo == idPrograma)?.IdPeriodo ?? 0;
                }

                var cargaModel = CargaAcademicaMapper.ToModel(carga);
                cargasModel.Add(cargaModel);
            }

            await _programacionAcademicaRepository
                .GuardarOfertasYCargas(ofertasModel, cargasModel);

            return true;
        }

        private static string ObtenerClavePrograma(string nombre)
        {
            return nombre.Split('-')[0].Trim();
        }

        public async Task<List<CargaConOfertaDTO>> ProcesarCargasAsync(
            IFormFile archivoCarga)
        {
            using var stream = archivoCarga.OpenReadStream();
            var cargas = CargasParser.Parse(stream);

           

            var resultado = new List<CargaConOfertaDTO>();

            foreach (var docente in cargas.Docentes)
            {
                foreach (var experiencia in docente.Experiencias)
                {

                    resultado.Add(new CargaConOfertaDTO
                    {
                        NumeroPersonal = docente.NumeroPersonal,
                        NombreDocente = docente.Nombre,
                        Plaza = string.IsNullOrWhiteSpace(experiencia.Plaza) ? null : experiencia.Plaza,
                        TipoContratacion = string.IsNullOrWhiteSpace(experiencia.TipoContratacion) ? null : experiencia.TipoContratacion,
                        Nrc = string.IsNullOrWhiteSpace(experiencia.Nrc) ? null : experiencia.Nrc,
                        ExperienciaEducativa = experiencia.ExperienciaEducativa,
                        HorasContacto = experiencia.HorasContacto,
                        HorasPago = experiencia.HorasPago,
                        Imparte = experiencia.Imparte,
                        Programa = experiencia.ClaveProgramatica,
                    });
                }
            }

            return resultado;
        }

        public async Task<List<EntidadAcademica>> ObtenerOpcionesEntidadAcademicaAsync(string region)
        {
            return await _entidadAcademicaRepository.ObtenerOpcionesAsync(region);
        }

        public async Task<List<ResumenOfertaProgramacionAcademicaDTO>> ObtenerResumenPorProgramaPeriodoAsync(BuscarProgramacionAcademicaDTO? filtro)
        {
            var resumen = await _programacionAcademicaRepository.ObtenerResumenPorProgramaPeriodoAsync(filtro);

            foreach (var item in resumen)
            {
                var periodoDto = PeriodoEscolarMapper.ToDTO(new Periodo
                {
                    IdPeriodo = item.IdPeriodo,
                    Codigo = item.CodigoPeriodo
                });

                item.PeriodoMostrar = periodoDto.PeriodoMostrar;
            }

            return resumen;
        }

        public async Task<List<ProgramaEducativo>> ObtenerOpcionesProgramaEducativoAsync(int idEntidadAcademica)
        {
            return await _programaEducativoRepository.ObtenerPorFiltroAsync(new BuscarProgramaEducativoDTO { IdEntidadAcademica = idEntidadAcademica });
        }

        public async Task<List<OfertaDTO>> ObtenerOfertasGuardadasAsync(
    int idEntidadAcademica, int idProgramaEducativo, int idPeriodo)
        {
            return await _programacionAcademicaRepository
                .ObtenerOfertasGuardadasAsync(idEntidadAcademica, idProgramaEducativo, idPeriodo);
        }

        public async Task<OfertaDTO?> ObtenerOfertaPorId(int idOferta)
        {
            return await _programacionAcademicaRepository.ObtenerOfertaPorId(idOferta);
        }

        public async Task<bool> EditarOfertaAsync(int idOferta, OfertaDTO ofertaDTO)
        {
            return await _programacionAcademicaRepository.EditarOfertaAsync(idOferta, ofertaDTO);
        }

        public async Task<List<LogDTO>> ObtenerHistorialPorIdOferta(int idOferta)
        {
            var movimientos = _programacionAcademicaRepository.ObtenerLogsPorOfertaAsync(idOferta);

            List<LogDTO> logs = new List<LogDTO>();

            foreach (var movimiento in await movimientos)
            {
                logs.Add(new LogDTO
                {
                    Fecha = movimiento.Fecha,
                    Mensaje = movimiento.Mensaje
                });
            }
            return logs;
        }

        public async Task<List<Log>> EliminarOfertaAsync(int idOferta)
        {
            await _programacionAcademicaRepository.EliminarOfertaAsync(idOferta);
            return await _programacionAcademicaRepository.ObtenerLogsPorOfertaAsync(idOferta);
        }

        public async Task CambiarInclusionOfertaAsync(int idOferta, bool incluir)
        {
            bool cambio = !incluir;

            await _programacionAcademicaRepository.CambiarInclusionOfertaAsync(idOferta, cambio);
        }

        public async Task CambiarAVacanteAsync(int idOferta, string justificacion)
        {
            await _programacionAcademicaRepository.CambiarAVacanteAsync(idOferta, justificacion);
        }

        public async Task AsignarDocenteAsync(int idOferta, int idDocente)
        {
            await _programacionAcademicaRepository.AsignarDocenteAsync(idOferta, idDocente);
        }
    }
}
