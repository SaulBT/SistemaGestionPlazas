using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface IDocenteRepository
    {
        Task<Docente> RegistrarAsync(Docente docente);
        Task<Docente?> ObtenerPorIdAsync(int idDocente);
        Task<List<Docente>> ObtenerTodosAsync();
        Task<List<Docente>> ObtenerPorPaginaAsync(string busqueda, int pagina, int cantidad);
        Task EditarAsync(Docente docenteEditado);
        Task EliminarAsync(Docente docente);
        Task<int> ContarAsync(string busqueda);
        Task<bool> ExistePorIdAsync(int idDocente);
        Task<bool> ExistePorNumeroAsync(string numeroPersonal);
        //obtener los np que sí están registrados en la base de datos, dado una lista de np
        Task<List<string>> ObtenerNumerosPersonalRegistradosAsync(
        List<string> numerosPersonal);


        Task<Dictionary<string, int>> ObtenerIdsPorNumeroPersonalAsync(List<string> numerosPersonal);


    }
}
