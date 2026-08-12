namespace SGPla.Services.Interfaces
{
    public interface ILdapAuthService
    {
        bool Autenticar(string username, string password);
    }
}
