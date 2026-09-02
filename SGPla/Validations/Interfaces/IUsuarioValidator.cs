using SGPla.Models.DTOs.Usuarios;

namespace SGPla.Validations.Interfaces
{
    public interface IUsuarioValidator
    {
        Task ValidarCreacionAsync(CrearUsuarioDTO creaUsuarioDTO);
        Task ValidarReferenciaAsync(ReferenciaUsuarioDTO referenciaUsuarioDTO);
        Task ValidarEdicionAsync(EditarUsuarioDTO editarUsuarioDTO);
    }
}
