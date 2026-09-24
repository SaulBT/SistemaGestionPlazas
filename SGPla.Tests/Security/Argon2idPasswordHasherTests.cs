using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Security;

public sealed class Argon2idPasswordHasherTests
{
    private readonly Argon2idPasswordHasher _hasher = new();

    [Fact]
    public void Hash_UsaFormatoPhcArgon2idYVerificaLaContrasena()
    {
        const string password = "una-frase secreta de prueba";
        var phc = _hasher.Hash(password);

        Assert.StartsWith("$argon2id$v=19$m=19456,t=2,p=1$", phc);
        Assert.True(phc.Length <= 500);
        Assert.True(_hasher.Verify(password, phc).EsCorrecta);
        Assert.False(_hasher.Verify("otra contraseña", phc).EsCorrecta);
    }

    [Fact]
    public void Hash_UsaUnSaltAleatorioYDetectaHashDesactualizado()
    {
        const string password = "frase de acceso de prueba";
        var first = _hasher.Hash(password);
        var second = _hasher.Hash(password);

        Assert.NotEqual(first, second);
        Assert.False(_hasher.Verify(password, first).RequiereRehash);

        var oldPhc = CrearHashAntiguo(password);
        var verification = _hasher.Verify(password, oldPhc);
        Assert.True(verification.EsCorrecta);
        Assert.True(verification.RequiereRehash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("texto-no-phc")]
    [InlineData("$argon2d$v=19$m=19456,t=2,p=1$AA$AA")]
    [InlineData("$argon2id$v=19$m=999999999,t=2,p=1$AA$AA")]
    [InlineData("$argon2id$v=19$m=19456,t=2,p=1$not-base64$AA")]
    public void Verify_RechazaPHCMalformadoOSinLimites(string phc) =>
        Assert.False(_hasher.Verify("password", phc).EsCorrecta);

    private static string CrearHashAntiguo(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        try
        {
            using var argon2 = new Argon2id(passwordBytes)
            {
                Salt = salt,
                MemorySize = 8_192,
                Iterations = 1,
                DegreeOfParallelism = 1
            };
            var hash = argon2.GetBytes(32);
            return $"$argon2id$v=19$m=8192,t=1,p=1${Convert.ToBase64String(salt).TrimEnd('=')}${Convert.ToBase64String(hash).TrimEnd('=')}";
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
        }
    }
}
