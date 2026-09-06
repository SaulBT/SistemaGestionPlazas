namespace SGPla.Modules.PeriodosEscolares.Domain;

public sealed record PeriodoEscolarValidacion(
    string Campo,
    string Mensaje);

public sealed record PeriodoEscolarDetalle(
    int IdPeriodoEscolar,
    string Codigo,
    string Periodo,
    int Anio,
    string PeriodoMostrar);

public static class PeriodoEscolarReglas
{
    public const int ANIO_MINIMO = 2000;
    public const int ANIO_MAXIMO = 2100;
    public const int PAGINA_POR_DEFECTO = 1;
    public const int CANTIDAD_POR_DEFECTO = 10;
    public const int CANTIDAD_MAXIMA = 100;

    public static PeriodoEscolarValidacion? ValidarDatos(
        int? anio,
        string? periodo)
    {
        if (!anio.HasValue)
        {
            return new PeriodoEscolarValidacion(
                "anio",
                "El año es obligatorio.");
        }

        if (anio < ANIO_MINIMO || anio > ANIO_MAXIMO)
        {
            return new PeriodoEscolarValidacion(
                "anio",
                "El año debe estar entre 2000 y 2100.");
        }

        if (string.IsNullOrWhiteSpace(periodo))
        {
            return new PeriodoEscolarValidacion(
                "periodo",
                "El periodo es obligatorio.");
        }

        if (!TryObtenerCodigoPeriodo(periodo, out _))
        {
            return new PeriodoEscolarValidacion(
                "periodo",
                "El periodo debe ser Febrero-Julio o Agosto-Enero.");
        }

        return null;
    }

    public static PeriodoEscolarValidacion? ValidarConsulta(
        int? anio,
        string? periodo,
        int pagina,
        int cantidad)
    {
        if (anio.HasValue && (anio < ANIO_MINIMO || anio > ANIO_MAXIMO))
        {
            return new PeriodoEscolarValidacion(
                "anio",
                "El año debe estar entre 2000 y 2100.");
        }

        if (!string.IsNullOrWhiteSpace(periodo)
            && !TryObtenerCodigoPeriodo(periodo, out _))
        {
            return new PeriodoEscolarValidacion(
                "periodo",
                "El periodo debe ser Febrero-Julio o Agosto-Enero.");
        }

        if (pagina <= 0)
        {
            return new PeriodoEscolarValidacion(
                "pagina",
                "La página debe ser mayor que cero.");
        }

        if (cantidad <= 0 || cantidad > CANTIDAD_MAXIMA)
        {
            return new PeriodoEscolarValidacion(
                "cantidad",
                "La cantidad debe estar entre 1 y 100.");
        }

        return null;
    }

    public static bool TryObtenerCodigoPeriodo(
        string? periodo,
        out string codigo)
    {
        switch (periodo?.Trim())
        {
            case "Febrero-Julio":
                codigo = "51";
                return true;
            case "Agosto-Enero":
                codigo = "01";
                return true;
            default:
                codigo = string.Empty;
                return false;
        }
    }

    public static string ConstruirCodigo(
        int anio,
        string periodo)
    {
        if (!TryObtenerCodigoPeriodo(periodo, out var codigoPeriodo))
        {
            throw new ArgumentException("El periodo no es válido.", nameof(periodo));
        }

        return $"{anio:D4}{codigoPeriodo}";
    }

    public static PeriodoEscolarDetalle CrearDetalle(
        int idPeriodoEscolar,
        string codigo)
    {
        var codigoNormalizado = codigo.Trim();

        if (codigoNormalizado.Length != 6
            || !int.TryParse(codigoNormalizado[..4], out var anio))
        {
            throw new InvalidOperationException(
                "El código almacenado del periodo escolar no es válido.");
        }

        var codigoPeriodo = codigoNormalizado[4..];
        var periodo = codigoPeriodo switch
        {
            "01" => "Agosto-Enero",
            "51" => "Febrero-Julio",
            _ => null
        };

        if (periodo is null)
        {
            throw new InvalidOperationException(
                "El código almacenado del periodo escolar no contiene un periodo válido.");
        }

        var periodoMostrar = codigoPeriodo switch
        {
            "01" => $"Agosto {anio - 1} – Enero {anio}",
            "51" => $"Febrero – Julio {anio}",
            _ => throw new InvalidOperationException(
                "El código almacenado del periodo escolar no contiene un periodo válido.")
        };

        return new PeriodoEscolarDetalle(
            idPeriodoEscolar,
            codigoNormalizado,
            periodo,
            anio,
            periodoMostrar);
    }
}
