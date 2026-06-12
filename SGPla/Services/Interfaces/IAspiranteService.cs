using SGPla.Models.DTOs.Docentes;

namespace SGPla.Services.Interfaces
{
    public interface IAspiranteService
    {
        Task RegistrarAspiranteAsync(RegistrarDocenteDTO dto);
        Task<DatosDocenteDTO> ObtenerAspiranteAsync(int idDocente);
        Task<(List<ListaAspiranteDTO> items, int total)> ObtenerTodosAspirantesAsync(string busqueda, int pagina, int cantidad);
        Task EditarAspiranteAsync(EditarDocenteDTO dto);
        Task EliminarAspiranteAsync(int idDocente);
    }
}
