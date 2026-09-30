using SGPla.Models.DTOs.Articulo;
using SGPla.Models.DTOs.ProgramaEducativo;

namespace SGPla.Validations.Interfaces
{
    public interface IProgramaEducativoValidator
    {
        Task ValidarCreacionAsync(CrearProgramaEducativoDTO crearProgramaEducativoDTO);

        Task ValidarEdicionAsync(EditarProgramaEducativoDTO editarProgramaEducativoDTO);

        Task ValidarBusquedaPorFiltroAsync(BuscarProgramaEducativoDTO buscarProgramaEducativoDTO);

        Task ValidarObtenerPorIdAsync(int id);

        Task ValidarEliminarAsync(int id);
    }
}
