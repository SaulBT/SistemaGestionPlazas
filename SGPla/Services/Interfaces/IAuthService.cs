using SGPla.Models.DTOs.Auth;

namespace SGPla.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ResultadoAutenticacion> LoginAsync(string username, string password);
    }
}
