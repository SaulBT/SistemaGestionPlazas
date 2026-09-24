using SGPla.Models.DTOs.Auth;

namespace SGPla.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ResultadoAutenticacion> LoginAsync(string username, string password, CancellationToken cancellationToken = default);

        Task<bool> CambiarContrasenaSuperusuarioAsync(int usuarioId, string contrasenaActual,
            string contrasenaNueva, CancellationToken cancellationToken = default);
    }
}
