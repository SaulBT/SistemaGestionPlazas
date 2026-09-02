using SGPla.Models.DTOs.IntegranteCt;

namespace SGPla.Validations.Interfaces
{
    public interface IIntegranteCtValidator
    {
        Task ValidarRegistroAsync(RegistrarIntegranteCtDto integranteDto);
        Task ValidarEdicionAsync(DatosIntegranteCtDto integranteDto);
        Task ValidarIdIntegranteAsync(int idIntegrante);
        Task ValidarIdEntidadAsync(int idEntidad);
    }
}
