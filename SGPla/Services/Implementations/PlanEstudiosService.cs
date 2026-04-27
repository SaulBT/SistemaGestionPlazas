using ClosedXML.Excel;
using SGPla.Models;
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

        public PlanEstudiosService(
            IPlanEstudiosRepository planEstudiosRepository,
            IExperienciaEducativaRepository experienciaEducativaRepository,
            IPlanEstudiosValidator planEstudiosValidator)
        {
            _planEstudiosRepository = planEstudiosRepository;
            _experienciaEducativaRepository = experienciaEducativaRepository;
            _planEstudiosValidator = planEstudiosValidator;
        }

        public List<DatosExperienciaEducativaDTO> ProcesarArchivo(ArchivoPlanEstudiosDTO archivoPlanEstudiosDTO)
        {
            _planEstudiosValidator.ValidarArchivo(archivoPlanEstudiosDTO);

            archivoPlanEstudiosDTO.Archivo.Position = 0;

            using var workbook = new XLWorkbook(archivoPlanEstudiosDTO.Archivo);
            var worksheet = workbook.Worksheet(1);
            var experienciasEducativas = new List<DatosExperienciaEducativaDTO>();

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                var materia = row.Cell(6).GetFormattedString().Trim();
                var curso = row.Cell(7).GetFormattedString().Trim();
                var nombre = row.Cell(8).GetFormattedString().Trim();
                var perfilDocente = row.Cell(14).GetFormattedString().Trim();

                if (string.IsNullOrWhiteSpace(materia)
                    && string.IsNullOrWhiteSpace(curso)
                    && string.IsNullOrWhiteSpace(nombre)
                    && string.IsNullOrWhiteSpace(perfilDocente))
                {
                    continue;
                }

                var codigo = string.Concat(materia, " ", curso).Trim();

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

            var planEstudios = new PlanEstudios
            {
                IdProgramaEducativo = crearPlanEstudiosDTO.IdProgramaEducativo,
                Nombre = crearPlanEstudiosDTO.Nombre,
                Modalidad = crearPlanEstudiosDTO.Sistema
            };

            var planEstudiosCreado = await _planEstudiosRepository.CrearAsync(planEstudios);
            var experienciasEducativas = mapearExperienciasNuevas(crearPlanEstudiosDTO.ExperienciasEducativas, planEstudiosCreado.IdPlanEstudios);

            await _experienciaEducativaRepository.CrearExperienciasEducativasAsync(experienciasEducativas);

            return planEstudiosCreado.IdPlanEstudios;
        }

        public async Task<List<ListaPlanEstudiosDTO>> ObtenerTodosAsync()
        {
            var planesEstudios = await _planEstudiosRepository.ObtenerTodosAsync();
            var dtos = planesEstudios.Select(mapearLista);

            return dtos.ToList();
        }

        public async Task<List<ListaPlanEstudiosDTO>> ObtenerDiezAsync(int indice)
        {
            _planEstudiosValidator.ValidarIndice(indice);

            var planesEstudios = await _planEstudiosRepository.ObtenerDiezAsync(indice);
            var dtos = planesEstudios.Select(mapearLista);

            return dtos.ToList();
        }

        public async Task<List<ListaPlanEstudiosDTO>> ObtenerPorFiltroAsync(FiltroPlanEstudiosDTO filtroPlanEstudiosDTO, int indice)
        {
            ArgumentNullException.ThrowIfNull(filtroPlanEstudiosDTO);
            _planEstudiosValidator.ValidarIndice(indice);

            var planesEstudios = await _planEstudiosRepository.ObtenerPorFiltroAsync(
                filtroPlanEstudiosDTO.IdEntidadAcademica,
                filtroPlanEstudiosDTO.IdProgramaEducativo,
                filtroPlanEstudiosDTO.Nombre,
                indice);

            var dtos = planesEstudios.Select(mapearLista);
            return dtos.ToList();
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

            if (editarPlanEstudiosDTO.IdsExperienciasEliminadas.Count > 0)
                await _experienciaEducativaRepository.EliminarExperienciasEducativasPorIdsAsync(editarPlanEstudiosDTO.IdsExperienciasEliminadas);

            if (editarPlanEstudiosDTO.ExperienciasEditadas.Count > 0)
                await _experienciaEducativaRepository.ActualizarExperienciasEducativasAsync(
                    mapearExperienciasEditadas(editarPlanEstudiosDTO.ExperienciasEditadas, editarPlanEstudiosDTO.IdPlanEstudios));

            if (editarPlanEstudiosDTO.ExperienciasNuevas.Count > 0)
                await _experienciaEducativaRepository.CrearExperienciasEducativasAsync(
                    mapearExperienciasNuevas(editarPlanEstudiosDTO.ExperienciasNuevas, editarPlanEstudiosDTO.IdPlanEstudios));
        }

        public async Task EliminarAsync(int idPlanEstudios)
        {
            await _planEstudiosValidator.ValidarIdAsync(idPlanEstudios);

            var planEstudios = await _planEstudiosRepository.ObtenerPorIdAsync(idPlanEstudios);
            var experiencias = await _experienciaEducativaRepository.ObtenerExperienciasEducativasPorIdPlanEstudiosAsync(idPlanEstudios);
            var ids = experiencias.Select(experienciaEducativa => experienciaEducativa.IdExperienciaEducativa).ToList();
            await _experienciaEducativaRepository.EliminarExperienciasEducativasPorIdsAsync(ids);
            await _planEstudiosRepository.EliminarAsync(planEstudios!);
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
                ExperienciasEducativos = experienciasEducativas.Select(experienciaEducativa => new DatosExperienciaEducativaDTO
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
