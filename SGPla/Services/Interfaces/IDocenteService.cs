using SGPla.Models.DTOs.Docentes;

namespace SGPla.Services.Interfaces
{
    public interface IDocenteService
    {
        Task RegistrarDocenteAsync(RegistrarDocenteDTO docente);
        Task<DatosDocenteDTO> ObtenerDocenteAsync(int idDocente);
        Task<(List<ListaDocenteDTO> items, int total)> ObtenerTodosDocentesAsync(string busqueda, int pagina, int cantidad);
        Task EditarDocenteAsync(EditarDocenteDTO docente);
        Task EliminarDocenteAsync(int idDocente);
    }
}
