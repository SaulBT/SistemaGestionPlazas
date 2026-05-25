using SGPla.Models;

namespace SGPla.Repositories.Interfaces
{
    public interface IDocenteRepository
    {
        Task<List<string>> ObtenerNumerosPersonalRegistradosAsync(
        List<string> numerosPersonal);
    }
}
