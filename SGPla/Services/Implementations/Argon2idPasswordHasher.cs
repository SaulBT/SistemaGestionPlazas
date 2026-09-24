using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class Argon2idPasswordHasher : IArgon2idPasswordHasher
{
    private const int CurrentMemoryKiB = 19_456;
    private const int CurrentIterations = 2;
    private const int CurrentParallelism = 1;
    private const int SaltLength = 16;
    private const int HashLength = 32;
    private static readonly UTF8Encoding Utf8 = new(false, true);

    public string Hash(string password)
    {
        var passwordBytes = EncodePassword(password);
        var salt = RandomNumberGenerator.GetBytes(SaltLength);
        try
        {
            var hash = Derive(passwordBytes, salt, CurrentMemoryKiB, CurrentIterations, CurrentParallelism, HashLength);
            return $"$argon2id$v=19$m={CurrentMemoryKiB},t={CurrentIterations},p={CurrentParallelism}${EncodeBase64(salt)}${EncodeBase64(hash)}";
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
        }
    }

    public PasswordHashVerification Verify(string password, string phc)
    {
        if (string.IsNullOrEmpty(password) || !TryParse(phc, out var parsed))
            return new(false, false);

        byte[] passwordBytes;
        try
        {
            passwordBytes = EncodePassword(password);
        }
        catch (ArgumentException)
        {
            return new(false, false);
        }

        try
        {
            var actual = Derive(passwordBytes, parsed.Salt, parsed.MemoryKiB, parsed.Iterations,
                parsed.Parallelism, parsed.Hash.Length);
            try
            {
                var matches = CryptographicOperations.FixedTimeEquals(actual, parsed.Hash);
                var needsRehash = matches && (parsed.MemoryKiB != CurrentMemoryKiB ||
                    parsed.Iterations != CurrentIterations || parsed.Parallelism != CurrentParallelism ||
                    parsed.Salt.Length < SaltLength || parsed.Hash.Length != HashLength);
                return new(matches, needsRehash);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(actual);
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
            CryptographicOperations.ZeroMemory(parsed.Hash);
        }
    }

    private static byte[] EncodePassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        var byteCount = Utf8.GetByteCount(password);
        if (byteCount is < 1 or > 1024)
            throw new ArgumentOutOfRangeException(nameof(password), "La contraseña excede el tamaño admitido.");
        return Utf8.GetBytes(password);
    }

    private static byte[] Derive(byte[] password, byte[] salt, int memoryKiB, int iterations, int parallelism, int hashLength)
    {
        using var argon2 = new Argon2id(password)
        {
            Salt = salt,
            MemorySize = memoryKiB,
            Iterations = iterations,
            DegreeOfParallelism = parallelism
        };
        return argon2.GetBytes(hashLength);
    }

    private static string EncodeBase64(byte[] value) => Convert.ToBase64String(value).TrimEnd('=');

    private static bool TryParse(string? phc, out ParsedHash parsed)
    {
        parsed = default;
        if (string.IsNullOrWhiteSpace(phc) || phc.Length > 500)
            return false;

        var parts = phc.Split('$');
        if (parts.Length != 6 || parts[0].Length != 0 || parts[1] != "argon2id" || parts[2] != "v=19")
            return false;

        var parameters = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var pair in parts[3].Split(','))
        {
            var separator = pair.IndexOf('=');
            if (separator <= 0 || !int.TryParse(pair.AsSpan(separator + 1), NumberStyles.None,
                    CultureInfo.InvariantCulture, out var value) ||
                !parameters.TryAdd(pair[..separator], value))
                return false;
        }

        if (parameters.Count != 3 || !parameters.TryGetValue("m", out var memoryKiB) ||
            !parameters.TryGetValue("t", out var iterations) || !parameters.TryGetValue("p", out var parallelism) ||
            memoryKiB is < 8_192 or > 262_144 || iterations is < 1 or > 10 || parallelism is < 1 or > 4 ||
            memoryKiB < 8 * parallelism)
            return false;

        byte[] salt;
        byte[] hash;
        try
        {
            salt = DecodeBase64(parts[4]);
            hash = DecodeBase64(parts[5]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (salt.Length is < 8 or > 64 || hash.Length is < 16 or > 64)
        {
            CryptographicOperations.ZeroMemory(hash);
            return false;
        }

        parsed = new ParsedHash(memoryKiB, iterations, parallelism, salt, hash);
        return true;
    }

    private static byte[] DecodeBase64(string value)
    {
        if (value.Length == 0 || value.Contains('=') || value.Any(char.IsWhiteSpace))
            throw new FormatException("La codificación PHC contiene Base64 no canónico.");
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + ((4 - padded.Length % 4) % 4), '=');
        return Convert.FromBase64String(padded);
    }

    private readonly record struct ParsedHash(int MemoryKiB, int Iterations, int Parallelism, byte[] Salt, byte[] Hash);
}
