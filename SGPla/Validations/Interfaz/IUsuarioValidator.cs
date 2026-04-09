using SGPla.Models.DTOs.Usuarios;

namespace SGPla.Validations.Interfaz
{
    public interface IUsuarioValidator
    {
        Task ValidarCreacionAsync(CrearUsuarioDTO creaUsuarioDTO);
    }
}
