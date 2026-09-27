using System.DirectoryServices.Protocols;
using System.Net;

namespace SGPla.Tests.Integration;

public sealed class LdapStartTlsIntegrationTests
{
    [LdapStartTlsFact]
    public void ServidorInstitucional_NegociaStartTlsSinEnviarCredenciales()
    {
        var host = Environment.GetEnvironmentVariable("SGPLA_LDAP_STARTTLS_HOST");
        Assert.False(string.IsNullOrWhiteSpace(host));

        var port = int.TryParse(Environment.GetEnvironmentVariable("SGPLA_LDAP_STARTTLS_PORT"), out var configuredPort)
            ? configuredPort : 389;
        using var connection = new LdapConnection(new LdapDirectoryIdentifier(host, port))
        {
            AuthType = AuthType.Basic,
            Timeout = TimeSpan.FromSeconds(10)
        };
        connection.SessionOptions.ProtocolVersion = 3;
        connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;

        // Negocia TLS y valida el certificado con la política de confianza del sistema.
        // La prueba no configura Credential ni llama a Bind.
        connection.SessionOptions.StartTransportLayerSecurity(null);
    }

    private sealed class LdapStartTlsFactAttribute : FactAttribute
    {
        public LdapStartTlsFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SGPLA_LDAP_STARTTLS_HOST")))
                Skip = "Configura SGPLA_LDAP_STARTTLS_HOST para probar el endpoint institucional sin credenciales.";
        }
    }
}
