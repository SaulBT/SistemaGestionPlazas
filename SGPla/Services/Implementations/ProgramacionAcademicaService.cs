using SGPla.Mappers;
using SGPla.Models;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.ProgramacionAcademica;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Repositories.Interfaces;

namespace SGPla.Services.Implementations
{
    public class ProgramacionAcademicaService : IProgramacionAcademicaService
    {
        public readonly IProgramacionAcademicaRepository _programacionAcademicaRepository;
        public readonly IEntidadAcademicaRepository _entidadAcademicaRepository;
        public readonly IProgramaEducativoRepository _programaEducativoRepository;

        public ProgramacionAcademicaService(IProgramacionAcademicaRepository programacionAcademicaRepository, IEntidadAcademicaRepository entidadAcademicaRepository, IProgramaEducativoRepository programaEducativoRepository)
        {
            _programacionAcademicaRepository = programacionAcademicaRepository;
            _entidadAcademicaRepository = entidadAcademicaRepository;
            _programaEducativoRepository = programaEducativoRepository;
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

        public async Task<List<OfertaDTO>> ObtenerOfertasExperienciasEducativasAsync(
    int idEntidadAcademica, int idProgramaEducativo, int idPeriodo, string? busqueda)
        {
            return await _programacionAcademicaRepository
                .ObtenerOfertasExperienciasEducativasAsync(idEntidadAcademica, idProgramaEducativo, idPeriodo, busqueda);
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
