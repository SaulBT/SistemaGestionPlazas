using Microsoft.Identity.Client;
using SGPla.Models;
using SGPla.Models.DTOs.AreaAcademica;
using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Models.DTOs.ProgramaEducativo;

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

        Task<(List<DetallesProgramaEducativoDTO> Items, int TotalCount)> BuscarPorFiltroPaginadoAsync(BuscarProgramaEducativoDTO filtro);

        Task<List<EntidadAcademica>> ObtenerOpcionesEntidadAcademicaAsync(string region, int idAreaAcademica);

        Task<List<AreaAcademica>> ObtenerOpcionesAreaAcademicaAsync();
    }
}
