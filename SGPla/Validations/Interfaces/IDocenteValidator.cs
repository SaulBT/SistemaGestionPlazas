using SGPla.Models.DTOs.Docentes;

namespace SGPla.Validations.Interfaces
{
    public interface IDocenteValidator
    {
        Task ValidarRegistroAsync(RegistrarDocenteDTO dto);
        Task ValidarIdAsync(int idDocente);
        Task ValidarEdicionAsync(EditarDocenteDTO dto);
    }
}
