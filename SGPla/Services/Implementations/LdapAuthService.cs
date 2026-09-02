using SGPla.Services.Interfaces;
using System.DirectoryServices.Protocols;
using System.Net;

namespace SGPla.Services.Implementations
{
    public class LdapAuthService : ILdapAuthService
    {
        private readonly ILogger<LdapAuthService> _logger;
        private readonly string _servidor;
        private readonly int _puerto;

        public LdapAuthService(ILogger<LdapAuthService> logger, IConfiguration config)
        {
            _logger = logger;
            _servidor = config["Ldap:Servidor"]!; 
            _puerto = int.Parse(config["Ldap:Puerto"] ?? "389");
        }

        public bool Autenticar(string username, string password)
        {
            var usuarioLdap = username.Contains('@') ? username : $"{username}@uv.mx";

            try
            {
                _logger.LogInformation("Intentando autenticación LDAP para {Usuario}", usuarioLdap);

                using var connection = new LdapConnection(new LdapDirectoryIdentifier(_servidor, _puerto));
                connection.AuthType = AuthType.Basic;
                connection.SessionOptions.ProtocolVersion = 3;
                connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None; 

                connection.Credential = new NetworkCredential(usuarioLdap, password);
                connection.Bind();

                _logger.LogInformation("LDAP login exitoso para {Usuario}", usuarioLdap);
                return true;
            }
            catch (LdapException ex)
            {
                _logger.LogWarning("LDAP login fallido para {Usuario}: {Mensaje} (código {ErrorCode})", usuarioLdap, ex.Message, ex.ErrorCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado conectando a LDAP para {Usuario}", usuarioLdap);
                return false;
            }
        }
    }
}