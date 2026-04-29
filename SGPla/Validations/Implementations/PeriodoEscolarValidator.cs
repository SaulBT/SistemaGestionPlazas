using SGPla.Mappers;
using SGPla.Models.DTOs.PeriodoEscolar;
using SGPla.Validations.Interfaces;
using static SGPla.Mappers.PeriodoEscolarMapper;

namespace SGPla.Validations.Implementations
{
    public class PeriodoEscolarValidator : IPeriodoEscolarValidator
    {
        public Task ValidarBusquedaPorFiltroAsync(BuscarPeriodoEscolarDTO buscarPeriodoEscolarDTO)
        {
            if (buscarPeriodoEscolarDTO.Periodo != null)
            {
                buscarPeriodoEscolarDTO.PeriodoCodigo = PeriodoMapper.NombreACodigo.TryGetValue(buscarPeriodoEscolarDTO.Periodo, out var periodoCodigo) ? periodoCodigo : null;
            }

            if (buscarPeriodoEscolarDTO.Anio is not null && buscarPeriodoEscolarDTO.Anio.Value > 0)
            {
                buscarPeriodoEscolarDTO.AnioCodigo = buscarPeriodoEscolarDTO.Anio + 1;
            }

            return Task.CompletedTask;
        }

        public Task ValidarCreacionAsync(CrearPeriodoEscolarDTO crearPeriodoEscolarDTO)
        {
            return Task.CompletedTask;
        }

        public Task ValidarEdicionAsync(EditarPeriodoEscolarDTO editarPeriodoEscolarDTO)
        {
            return Task.CompletedTask;
        }

        public Task ValidarEliminarAsync(int id)
        {
            return Task.CompletedTask;
        }

        public Task ValidarObtenerPorIdAsync(int id)
        {
            return Task.CompletedTask;
        }
    }
}
