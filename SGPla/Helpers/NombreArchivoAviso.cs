using System.Globalization;
using System.Text;

namespace SGPla.Helpers;

public static class NombreArchivoAviso
{
    private static readonly HashSet<string> Conectores = new(StringComparer.OrdinalIgnoreCase)
    {
        "DE", "DEL", "LA", "LAS", "LOS", "Y", "E"
    };

    public static string Construir(
        string codigoPeriodo,
        string articulo,
        string entidadAcademica,
        DateOnly fechaCreacion,
        int idAviso,
        string extension)
    {
        if (idAviso <= 0)
            throw new ArgumentOutOfRangeException(nameof(idAviso), "El identificador del aviso debe ser mayor que cero.");

        var periodoNormalizado = NormalizarSegmento(codigoPeriodo, "PERIODO");
        var articuloNormalizado = NormalizarArticulo(articulo);
        var siglasEntidad = GenerarSiglasEntidad(entidadAcademica);
        var fecha = fechaCreacion.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var extensionNormalizada = NormalizarExtension(extension);

        return $"AV-{periodoNormalizado}-{articuloNormalizado}-{siglasEntidad}-{fecha}-{idAviso}.{extensionNormalizada}";
    }

    public static string GenerarSiglasEntidad(string entidadAcademica)
    {
        var nombre = QuitarClaveInicial(entidadAcademica);
        var nombreNormalizado = QuitarAcentos(nombre).ToUpperInvariant();
        var palabras = SepararPalabras(nombreNormalizado)
            .Where(palabra => !Conectores.Contains(palabra));

        var siglas = string.Concat(palabras.Select(palabra => palabra[0]));
        return string.IsNullOrEmpty(siglas) ? "EA" : siglas;
    }

    private static string NormalizarArticulo(string articulo)
    {
        var articuloSinAcentos = QuitarAcentos(articulo).Trim();
        var primerDigito = articuloSinAcentos.IndexOfAny("0123456789".ToCharArray());
        if (primerDigito >= 0)
            articuloSinAcentos = articuloSinAcentos[primerDigito..];

        return NormalizarSegmento(articuloSinAcentos, "ARTICULO");
    }

    private static string NormalizarExtension(string extension)
    {
        var extensionNormalizada = extension.Trim().TrimStart('.').ToLowerInvariant();
        return extensionNormalizada switch
        {
            "pdf" => "pdf",
            "docx" => "docx",
            _ => throw new ArgumentException("La extensión del aviso debe ser PDF o DOCX.", nameof(extension))
        };
    }

    private static string NormalizarSegmento(string valor, string valorAlternativo)
    {
        var texto = QuitarAcentos(valor).ToUpperInvariant();
        var resultado = new StringBuilder(texto.Length);
        var requiereGuion = false;

        foreach (var caracter in texto)
        {
            var permitido = caracter is >= 'A' and <= 'Z' or >= '0' and <= '9';
            if (permitido)
            {
                if (requiereGuion && resultado.Length > 0)
                    resultado.Append('-');
                resultado.Append(caracter);
                requiereGuion = false;
            }
            else
            {
                requiereGuion = resultado.Length > 0;
            }
        }

        return resultado.Length == 0 ? valorAlternativo : resultado.ToString();
    }

    private static string QuitarClaveInicial(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return string.Empty;

        var nombreLimpio = nombre.Trim();
        var separador = nombreLimpio.IndexOf('-');
        if (separador > 0 && nombreLimpio[..separador].Trim().All(char.IsDigit))
            return nombreLimpio[(separador + 1)..].Trim();

        return nombreLimpio;
    }

    private static IEnumerable<string> SepararPalabras(string valor)
    {
        var palabra = new StringBuilder();
        foreach (var caracter in valor)
        {
            if (caracter is >= 'A' and <= 'Z' or >= '0' and <= '9')
            {
                palabra.Append(caracter);
            }
            else if (palabra.Length > 0)
            {
                yield return palabra.ToString();
                palabra.Clear();
            }
        }

        if (palabra.Length > 0)
            yield return palabra.ToString();
    }

    private static string QuitarAcentos(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return string.Empty;

        var normalizado = valor.Normalize(NormalizationForm.FormD);
        var resultado = new StringBuilder(normalizado.Length);
        foreach (var caracter in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
                resultado.Append(caracter);
        }

        return resultado.ToString().Normalize(NormalizationForm.FormC);
    }
}
