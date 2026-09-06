namespace SGPla.Modules.Articulos.Domain;

public sealed record ArticuloValidacion(string Campo, string Mensaje);

public static class ArticuloReglas
{
    public const int LONGITUD_MAXIMA_NUMERO = 50;

    public static (string? Numero, string? Descripcion) Normalizar(
        string? numero,
        string? descripcion)
    {
        return (
            string.IsNullOrWhiteSpace(numero) ? null : numero.Trim(),
            string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim());
    }

    public static ArticuloValidacion? Validar(
        string? numero,
        string? descripcion)
    {
        if (string.IsNullOrWhiteSpace(numero))
        {
            return new ArticuloValidacion(
                "numero",
                "El número del artículo es obligatorio.");
        }

        if (numero.Length > LONGITUD_MAXIMA_NUMERO)
        {
            return new ArticuloValidacion(
                "numero",
                "El número del artículo no puede exceder los 50 caracteres.");
        }

        if (!numero.Any(char.IsDigit))
        {
            return new ArticuloValidacion(
                "numero",
                "El número del artículo debe contener al menos un número.");
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            return new ArticuloValidacion(
                "descripcion",
                "La descripción del artículo es obligatoria.");
        }

        return null;
    }
}
