using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface IDocenteRepository
    {
        //obtener los np que sí están registrados en la base de datos, dado una lista de np
        Task<List<string>> ObtenerNumerosPersonalRegistradosAsync(
        List<string> numerosPersonal);


        Task<Dictionary<string, int>> ObtenerIdsPorNumeroPersonalAsync(List<string> numerosPersonal);


    }
}
