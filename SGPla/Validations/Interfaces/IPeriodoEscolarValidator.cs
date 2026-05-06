using SGPla.Models.DTOs.PeriodoEscolar;

namespace SGPla.Validations.Interfaces
{
    public interface IPeriodoEscolarValidator
    {
        Task ValidarCreacionAsync(CrearPeriodoEscolarDTO crearPeriodoEscolarDTO);

        Task ValidarEdicionAsync(EditarPeriodoEscolarDTO editarPeriodoEscolarDTO    );

        Task ValidarBusquedaPorFiltroAsync(BuscarPeriodoEscolarDTO buscarPeriodoEscolarDTO  );

        Task ValidarObtenerPorIdAsync(int id);

        Task ValidarEliminarAsync(int id);
    }
}
