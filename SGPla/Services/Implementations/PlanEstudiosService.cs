using ExcelDataReader;
using SGPla.Models;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Services.Implementations
{
    public class PlanEstudiosService : IPlanEstudiosService
    {
        private readonly IPlanEstudiosRepository _planEstudiosRepository;
        private readonly IExperienciaEducativaRepository _experienciaEducativaRepository;
        private readonly IPlanEstudiosValidator _planEstudiosValidator;

        private readonly IArchivoRepository _archivoRepository;
        private readonly IArchivoService _archivoService;
        private readonly ILogger<PlanEstudiosService> _logger;

        public PlanEstudiosService(
            IPlanEstudiosRepository planEstudiosRepository,
            IExperienciaEducativaRepository experienciaEducativaRepository,
            IPlanEstudiosValidator planEstudiosValidator,
            IArchivoRepository archivoRepository,
            IArchivoService archivoService,
            ILogger<PlanEstudiosService> logger)
        {
            _planEstudiosRepository = planEstudiosRepository;
            _experienciaEducativaRepository = experienciaEducativaRepository;
            _planEstudiosValidator = planEstudiosValidator;
            _archivoRepository = archivoRepository;
            _archivoService = archivoService;
            _logger = logger;
        }

        public List<DatosExperienciaEducativaDTO> ProcesarArchivo(ArchivoPlanEstudiosDTO archivoPlanEstudiosDTO)
        {
            _logger.LogInformation("PLAN ESTUDIOS: Iniciando procesamiento del archivo de Plan de Estudios.");

            _planEstudiosValidator.ValidarArchivo(archivoPlanEstudiosDTO);

            var experienciasEducativas = new List<DatosExperienciaEducativaDTO>();

            using var stream = new FileStream(archivoPlanEstudiosDTO.Ruta, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);

            var esPrimeraFila = true;

            while (reader.Read())
            {
                if (esPrimeraFila)
                {
                    esPrimeraFila = false;
                    continue;
                }

                var materia = ObtenerTextoCelda(reader, 5);       // Columna 6: MATERIA_EE
                var curso = ObtenerTextoCelda(reader, 6);         // Columna 7: CURSO_EE
                var nombre = ObtenerTextoCelda(reader, 7);        // Columna 8: DESC_EE
                var perfilDocente = ObtenerTextoCelda(reader, 13); // Columna 14: PERFIL_DOC

                if (string.IsNullOrWhiteSpace(materia) &&
                    string.IsNullOrWhiteSpace(curso) &&
                    string.IsNullOrWhiteSpace(nombre) &&
                    string.IsNullOrWhiteSpace(perfilDocente))
                {
                    continue;
                }

                var codigo = $"{materia} {curso}".Trim();

                experienciasEducativas.Add(new DatosExperienciaEducativaDTO
                {
                    Codigo = codigo,
                    Nombre = nombre,
                    PerfilDocente = perfilDocente
                });
            }

            return experienciasEducativas;
        }

        public async Task<int> AgregarAsync(CrearPlanEstudiosDTO crearPlanEstudiosDTO)
        {
            await _planEstudiosValidator.ValidarCreacionAsync(crearPlanEstudiosDTO);

            DatosArchivoGuardadoDTO? archivoGuardado = null;
            Archivo? archivoRegistrado = null;

            try
            {
                archivoGuardado = await _archivoService.GuardarAsync(
                    crearPlanEstudiosDTO.Archivo.Ruta,
                    crearPlanEstudiosDTO.Archivo.NombreArchivo,
                    "planes-estudios");
                archivoRegistrado = await _archivoRepository.CrearAsync(new Archivo
                {
                    Nombre = archivoGuardado.NombreOriginal,
                    Ruta = archivoGuardado.Ruta,
                    Tipo = archivoGuardado.Tipo,
                    Tamanio = archivoGuardado.Tamanio
                });

                var planEstudios = new PlanEstudios
                {
                    IdProgramaEducativo = crearPlanEstudiosDTO.IdProgramaEducativo,
                    Nombre = crearPlanEstudiosDTO.Nombre,
                    Modalidad = crearPlanEstudiosDTO.Sistema,
                    IdArchivoPlan = archivoRegistrado.IdArchivo
                };

                var planEstudiosCreado = await _planEstudiosRepository.CrearAsync(planEstudios);
                var experienciasEducativas = mapearExperienciasNuevas(crearPlanEstudiosDTO.ExperienciasEducativas, planEstudiosCreado.IdPlanEstudios);

                await _experienciaEducativaRepository.CrearExperienciasEducativasAsync(experienciasEducativas);

                return planEstudiosCreado.IdPlanEstudios;
            }
            catch
            {
                if (archivoRegistrado != null)
                    await _archivoRepository.EliminarAsync(archivoRegistrado);
                if (archivoGuardado != null)
                    await _archivoService.EliminarAsync(archivoGuardado.Ruta);

                throw;
            }
        }

        public async Task<List<ListaPlanEstudiosDTO>> ObtenerTodosAsync()
        {
            var planesEstudios = await _planEstudiosRepository.ObtenerTodosAsync();
            var dtos = planesEstudios.Select(mapearLista);

            return dtos.ToList();
        }

        public async Task<(List<ListaPlanEstudiosDTO> items, int cantidad)> ObtenerPorPaginaAsync(int pagina, int cantidad)
        {
            _planEstudiosValidator.ValidarIndice(cantidad);

            var planesEstudios = await _planEstudiosRepository.ObtenerPorPaginaAsync(pagina, cantidad);
            var total = await _planEstudiosRepository.ContarAsync();

            var dtos = planesEstudios.Select(mapearLista);

            return (dtos.ToList(), total);
        }

        public async Task<(List<ListaPlanEstudiosDTO> items, int cantidad)> ObtenerPorFiltroAsync(FiltroPlanEstudiosDTO filtroPlanEstudiosDTO, int pagina, int cantidad)
        {
            _logger.LogInformation("PLAN ESTUDIOS: Obteniendo lista de Planes de Estudio por filtro.");

            ArgumentNullException.ThrowIfNull(filtroPlanEstudiosDTO);
            _planEstudiosValidator.ValidarIndice(cantidad);

            _logger.LogInformation("PLAN ESTUDIOS: Filtro recibido - IdEntidadAcademica: {IdEntidadAcademica}, IdProgramaEducativo: {IdProgramaEducativo}, Nombre: {Nombre}, Página: {Pagina}, Cantidad: {Cantidad}",
                filtroPlanEstudiosDTO.IdEntidadAcademica, filtroPlanEstudiosDTO.IdProgramaEducativo, filtroPlanEstudiosDTO.Nombre ,pagina, cantidad);

            var planesEstudios = await _planEstudiosRepository.ObtenerPorFiltroAsync(
                filtroPlanEstudiosDTO.IdEntidadAcademica,
                filtroPlanEstudiosDTO.IdProgramaEducativo,
                filtroPlanEstudiosDTO.Nombre,
                pagina, cantidad);
            var total = await _planEstudiosRepository.ContarPorFiltroAsync(
                filtroPlanEstudiosDTO.IdEntidadAcademica,
                filtroPlanEstudiosDTO.IdProgramaEducativo,
                filtroPlanEstudiosDTO.Nombre);

            var dtos = planesEstudios.Select(mapearLista);
            return (dtos.ToList(), total);
        }

        public async Task<DatosPlanEstudiosDTO> ObtenerPorIdAsync(int idPlanEstudios)
        {
            await _planEstudiosValidator.ValidarIdAsync(idPlanEstudios);

            var planEstudios = await _planEstudiosRepository.ObtenerPorIdAsync(idPlanEstudios);
            var experienciasEducativas = await _experienciaEducativaRepository.ObtenerExperienciasEducativasPorIdPlanEstudiosAsync(idPlanEstudios);

            return mapearDatos(planEstudios!, experienciasEducativas);
        }

        public async Task EditarAsync(EditarPlanEstudiosDTO editarPlanEstudiosDTO)
        {
            await _planEstudiosValidator.ValidarEdicionAsync(editarPlanEstudiosDTO);

            var planEstudios = await _planEstudiosRepository.ObtenerPorIdAsync(editarPlanEstudiosDTO.IdPlanEstudios);
            DatosArchivoGuardadoDTO? archivoNuevoGuardado = null;
            Archivo? archivoAnterior = null;
            string? rutaAnterior = null;

            try
            {
                if (editarPlanEstudiosDTO.NuevaLista && editarPlanEstudiosDTO.Archivo != null)
                {
                    archivoNuevoGuardado = await _archivoService.GuardarAsync(
                        editarPlanEstudiosDTO.Archivo.Ruta,
                        editarPlanEstudiosDTO.Archivo.NombreArchivo,
                        "planes-estudios");

                    if (planEstudios.IdArchivoPlan != 0)
                    {
                        archivoAnterior = await _archivoRepository.ObtenerPorIdAsync(planEstudios.IdArchivoPlan);

                        if (archivoAnterior != null)
                        {
                            rutaAnterior = archivoAnterior.Ruta;
                            archivoAnterior.Nombre = archivoNuevoGuardado.NombreOriginal;
                            archivoAnterior.Ruta = archivoNuevoGuardado.Ruta;
                            archivoAnterior.Tipo = archivoNuevoGuardado.Tipo;
                            archivoAnterior.Tamanio = archivoNuevoGuardado.Tamanio;

                            await _archivoRepository.ActualizarAsync(archivoAnterior);
                        }
                    }
                    else
                    {
                        var nuevoArchivo = await _archivoRepository.CrearAsync(new Archivo
                        {
                            Nombre = archivoNuevoGuardado.NombreOriginal,
                            Ruta = archivoNuevoGuardado.Ruta,
                            Tipo = archivoNuevoGuardado.Tipo,
                            Tamanio = archivoNuevoGuardado.Tamanio
                        });

                        planEstudios.IdArchivoPlan = nuevoArchivo.IdArchivo;
                        await _planEstudiosRepository.EditarAsync(planEstudios);
                    }
                }

                if (editarPlanEstudiosDTO.IdsExperienciasEliminadas.Count > 0)
                    await _experienciaEducativaRepository.EliminarExperienciasEducativasPorIdsAsync(editarPlanEstudiosDTO.IdsExperienciasEliminadas);

                if (editarPlanEstudiosDTO.ExperienciasEditadas.Count > 0)
                    await _experienciaEducativaRepository.ActualizarExperienciasEducativasAsync(
                        mapearExperienciasEditadas(editarPlanEstudiosDTO.ExperienciasEditadas, editarPlanEstudiosDTO.IdPlanEstudios));

                if (editarPlanEstudiosDTO.ExperienciasNuevas.Count > 0)
                    await _experienciaEducativaRepository.CrearExperienciasEducativasAsync(
                        mapearExperienciasNuevas(editarPlanEstudiosDTO.ExperienciasNuevas, editarPlanEstudiosDTO.IdPlanEstudios));

                if (!string.IsNullOrWhiteSpace(rutaAnterior) && archivoNuevoGuardado != null)
                    await _archivoService.EliminarAsync(rutaAnterior);
            }
            catch
            {
                if (archivoNuevoGuardado != null)
                    await _archivoService.EliminarAsync(archivoNuevoGuardado.Ruta);

                throw;
            }
            
        }

        public async Task EliminarAsync(int idPlanEstudios)
        {
            await _planEstudiosValidator.ValidarIdAsync(idPlanEstudios);

            var planEstudios = await _planEstudiosRepository.ObtenerPorIdAsync(idPlanEstudios);
            var experiencias = await _experienciaEducativaRepository.ObtenerExperienciasEducativasPorIdPlanEstudiosAsync(idPlanEstudios);
            var ids = experiencias.Select(experienciaEducativa => experienciaEducativa.IdExperienciaEducativa).ToList();
            var idArchivo = planEstudios.IdArchivoPlan;
            var archivo = await _archivoRepository.ObtenerPorIdAsync(idArchivo);

            await _experienciaEducativaRepository.EliminarExperienciasEducativasPorIdsAsync(ids);
            await _planEstudiosRepository.EliminarAsync(planEstudios!);
            await _archivoRepository.EliminarAsync(archivo!);
            await _archivoService.EliminarAsync(archivo.Ruta);
        }

        private static string ObtenerTextoCelda(IExcelDataReader reader, int columnIndex)
        {
            if (columnIndex >= reader.FieldCount || reader.IsDBNull(columnIndex))
                return string.Empty;

            return Convert.ToString(reader.GetValue(columnIndex))?.Trim() ?? string.Empty;
        }

        private List<ExperienciaEducativa> mapearExperienciasNuevas(List<AgregarExperienciaEducativaDTO> experienciasNuevas, int idPlanEstudios)
        {
            return experienciasNuevas.Select(experienciaEducativa => new ExperienciaEducativa
            {
                IdPlanEstudios = idPlanEstudios,
                Codigo = experienciaEducativa.Codigo.Trim(),
                Nombre = experienciaEducativa.Nombre.Trim(),
                PerfilDocente = experienciaEducativa.PerfilDocente.Trim()
            }).ToList();
        }

        private List<ExperienciaEducativa> mapearExperienciasEditadas(List<DatosExperienciaEducativaDTO> experienciasEditadas, int idPlanEstudios)
        {
            return experienciasEditadas.Select(experienciaEducativa => new ExperienciaEducativa
            {
                IdExperienciaEducativa = experienciaEducativa.IdExperienciaEducativa,
                IdPlanEstudios = idPlanEstudios,
                Codigo = experienciaEducativa.Codigo.Trim(),
                Nombre = experienciaEducativa.Nombre.Trim(),
                PerfilDocente = experienciaEducativa.PerfilDocente.Trim()
            }).ToList();
        }

        private ListaPlanEstudiosDTO mapearLista(PlanEstudios planEstudios)
        {
            return new ListaPlanEstudiosDTO
            {
                IdPlanEstudios = planEstudios.IdPlanEstudios,
                NombreProgramaEducativo = planEstudios.IdProgramaEducativoNavigation?.Nombre ?? string.Empty,
                Modalidad = planEstudios.Modalidad,
                Nombre = planEstudios.Nombre,
                Area = planEstudios.IdProgramaEducativoNavigation?.IdEntidadAcademicaNavigation?.IdAreaAcademicaNavigation?.Nombre ?? string.Empty
            };
        }

        private DatosPlanEstudiosDTO mapearDatos(PlanEstudios planEstudios, List<ExperienciaEducativa> experienciasEducativas)
        {
            return new DatosPlanEstudiosDTO
            {
                IdPlanEstudios = planEstudios.IdPlanEstudios,
                NombreProgramaEducativo = planEstudios.IdProgramaEducativoNavigation?.Nombre ?? string.Empty,
                Modalidad = planEstudios.Modalidad,
                Nombre = planEstudios.Nombre,
                NombreAreaAcademica = planEstudios.IdProgramaEducativoNavigation?.IdEntidadAcademicaNavigation?.IdAreaAcademicaNavigation?.Nombre ?? string.Empty,
                ExperienciasEducativas = experienciasEducativas.Select(experienciaEducativa => new DatosExperienciaEducativaDTO
                {
                    IdExperienciaEducativa = experienciaEducativa.IdExperienciaEducativa,
                    Codigo = experienciaEducativa.Codigo,
                    Nombre = experienciaEducativa.Nombre,
                    PerfilDocente = experienciaEducativa.PerfilDocente
                }).ToList()
            };
        }
    }
}
