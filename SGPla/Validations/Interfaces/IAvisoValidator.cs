using SGPla.Models.DTOs.Aviso;

namespace SGPla.Validations.Interfaces
{
    public interface IAvisoValidator
    {
        Task ValidarCrearAviso(CrearAvisoDTO dto);
    }
}
