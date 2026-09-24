namespace SGPla.Services.Interfaces;

public interface IArgon2idPasswordHasher
{
    string Hash(string password);
    PasswordHashVerification Verify(string password, string phc);
}

public sealed record PasswordHashVerification(bool EsCorrecta, bool RequiereRehash);
