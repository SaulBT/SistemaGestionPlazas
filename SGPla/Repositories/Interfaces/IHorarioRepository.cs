using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface IHorarioRepository
    {
        Task<List<Horario>> ObtenerPorIdOferta(int idOferta);
        Task<List<Horario>> ObtenerPorIdAviso(int idAviso);
        Task CrearHorarios(List<Horario> horarios);
        Task EliminarHorariosPorId(List<int> idsHorarios);
        Task ActualizarHorariosPorId(List<Horario> horarios);

        // Metodos para la validacion
        //Task<bool> ExisteHorario


    }
}
