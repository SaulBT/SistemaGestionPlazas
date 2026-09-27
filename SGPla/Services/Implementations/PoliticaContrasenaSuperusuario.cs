using System.Text;

namespace SGPla.Services.Implementations;

internal static class PoliticaContrasenaSuperusuario
{
    private static readonly UTF8Encoding Utf8Estricto = new(false, true);

    public static bool EsValida(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length is < 8 or > 128 ||
            !password.Any(char.IsUpper) || !password.Any(char.IsLower) ||
            !password.Any(char.IsDigit) || !password.Any(c => !char.IsLetterOrDigit(c)))
            return false;
        try { return Utf8Estricto.GetByteCount(password) <= 1024; }
        catch (EncoderFallbackException) { return false; }
    }
}
