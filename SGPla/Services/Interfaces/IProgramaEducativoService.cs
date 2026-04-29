using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Models.DTOs.AreaAcademica;

namespace SGPla.Services.Interfaces
{
    public interface IProgramaEducativoService
    {
        Task<List<DetallesProgramaEducativoDTO>> ObtenerTodosAsync();
        Task<DetallesProgramaEducativoDTO?> ObtenerPorIdAsync(int id);

        Task<DetallesProgramaEducativoDTO> CrearAsync(CrearProgramaEducativoDTO programaEducativo);

        Task<DetallesProgramaEducativoDTO> EditarAsync(EditarProgramaEducativoDTO programaEducativo);

        Task<bool> EliminarAsync(int id);

        Task<List<DetallesProgramaEducativoDTO>> BuscarPorFiltroAsync(BuscarProgramaEducativoDTO filtro);

        Task<List<OpcionEntidadAcademicaDTO>> ObtenerOpcionesEntidadAcademicaAsync(string region, int idAreaAcademica);


        Task<List<OpcionAreaAcademicaDTO>> ObtenerOpcionesAreaAcademicaAsync();
    }
}
