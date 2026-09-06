using SGPla.Commons;

namespace SGPla.Modules.EntidadesAcademicas.Domain;

public sealed record EntidadAcademicaValidacion(
    string Campo,
    string Mensaje);

public sealed record EntidadAcademicaDatosNormalizados(
    string Clave,
    string Nombre,
    string CalleNumero,
    string Colonia,
    string Cp,
    string Municipio,
    string Telefono,
    string Extension,
    int IdAreaAcademica,
    string Region);

public static class EntidadAcademicaReglas
{
    public const int LONGITUD_CLAVE = 5;
    public const int LONGITUD_MAXIMA_NOMBRE = 100;
    public const int LONGITUD_MAXIMA_CALLE_NUMERO = 150;
    public const int LONGITUD_MAXIMA_COLONIA = 100;
    public const int LONGITUD_MAXIMA_CP = 5;
    public const int LONGITUD_MAXIMA_MUNICIPIO = 100;
    public const int LONGITUD_MAXIMA_TELEFONO = 30;
    public const int LONGITUD_MAXIMA_EXTENSION = 5;
    public const int LONGITUD_MAXIMA_REGION = 30;
    public const int CANTIDAD_MAXIMA = 100;

    public static EntidadAcademicaValidacion? ValidarConsulta(
        string? region,
        int? idAreaAcademica,
        int pagina,
        int cantidad)
    {
        if (!string.IsNullOrWhiteSpace(region)
            && !Constantes.REGIONES.Contains(region.Trim()))
        {
            return new EntidadAcademicaValidacion(
                "region",
                "La región indicada no es válida.");
        }

        if (idAreaAcademica.HasValue && idAreaAcademica <= 0)
        {
            return new EntidadAcademicaValidacion(
                "idAreaAcademica",
                "El ID del área académica no es válido.");
        }

        if (pagina <= 0)
        {
            return new EntidadAcademicaValidacion(
                "pagina",
                "La página debe ser mayor que cero.");
        }

        if (cantidad <= 0 || cantidad > CANTIDAD_MAXIMA)
        {
            return new EntidadAcademicaValidacion(
                "cantidad",
                "La cantidad debe estar entre 1 y 100.");
        }

        return null;
    }

    public static EntidadAcademicaValidacion? ValidarDatos(
        string? clave,
        string? nombre,
        string? calleNumero,
        string? colonia,
        string? cp,
        string? municipio,
        string? telefono,
        string? extension,
        int? idAreaAcademica,
        string? region)
    {
        if (string.IsNullOrWhiteSpace(clave))
        {
            return new EntidadAcademicaValidacion(
                "clave",
                "La clave es obligatoria.");
        }

        if (clave.Trim().Length != LONGITUD_CLAVE
            || !clave.Trim().All(char.IsDigit))
        {
            return new EntidadAcademicaValidacion(
                "clave",
                "La clave debe contener exactamente 5 números.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return new EntidadAcademicaValidacion(
                "nombre",
                "El nombre es obligatorio.");
        }

        var validacionLongitud = ValidarLongitud(
            nombre,
            LONGITUD_MAXIMA_NOMBRE,
            "nombre",
            "El nombre no puede exceder los 100 caracteres.");
        if (validacionLongitud is not null)
        {
            return validacionLongitud;
        }

        validacionLongitud = ValidarTextoObligatorio(
            calleNumero,
            LONGITUD_MAXIMA_CALLE_NUMERO,
            "calleNumero",
            "La calle y número son obligatorios.",
            "La calle y número no pueden exceder los 150 caracteres.");
        if (validacionLongitud is not null)
        {
            return validacionLongitud;
        }

        validacionLongitud = ValidarTextoObligatorio(
            colonia,
            LONGITUD_MAXIMA_COLONIA,
            "colonia",
            "La colonia es obligatoria.",
            "La colonia no puede exceder los 100 caracteres.");
        if (validacionLongitud is not null)
        {
            return validacionLongitud;
        }

        validacionLongitud = ValidarTextoObligatorio(
            cp,
            LONGITUD_MAXIMA_CP,
            "cp",
            "El código postal es obligatorio.",
            "El código postal no puede exceder los 5 caracteres.");
        if (validacionLongitud is not null)
        {
            return validacionLongitud;
        }

        validacionLongitud = ValidarTextoObligatorio(
            municipio,
            LONGITUD_MAXIMA_MUNICIPIO,
            "municipio",
            "El municipio es obligatorio.",
            "El municipio no puede exceder los 100 caracteres.");
        if (validacionLongitud is not null)
        {
            return validacionLongitud;
        }

        validacionLongitud = ValidarTextoObligatorio(
            telefono,
            LONGITUD_MAXIMA_TELEFONO,
            "telefono",
            "El teléfono es obligatorio.",
            "El teléfono no puede exceder los 30 caracteres.");
        if (validacionLongitud is not null)
        {
            return validacionLongitud;
        }

        validacionLongitud = ValidarTextoObligatorio(
            extension,
            LONGITUD_MAXIMA_EXTENSION,
            "extension",
            "La extensión es obligatoria.",
            "La extensión no puede exceder los 5 caracteres.");
        if (validacionLongitud is not null)
        {
            return validacionLongitud;
        }

        if (!idAreaAcademica.HasValue || idAreaAcademica <= 0)
        {
            return new EntidadAcademicaValidacion(
                "idAreaAcademica",
                "El ID del área académica es obligatorio y debe ser válido.");
        }

        if (string.IsNullOrWhiteSpace(region))
        {
            return new EntidadAcademicaValidacion(
                "region",
                "La región es obligatoria.");
        }

        if (!Constantes.REGIONES.Contains(region.Trim()))
        {
            return new EntidadAcademicaValidacion(
                "region",
                "La región indicada no es válida.");
        }

        var prefijoEsperado = region.Trim()[0].ToString();
        if (!clave.Trim().StartsWith(prefijoEsperado, StringComparison.Ordinal))
        {
            return new EntidadAcademicaValidacion(
                "clave",
                "La clave no coincide con la región seleccionada.");
        }

        return null;
    }

    public static EntidadAcademicaDatosNormalizados Normalizar(
        string clave,
        string nombre,
        string calleNumero,
        string colonia,
        string cp,
        string municipio,
        string telefono,
        string extension,
        int idAreaAcademica,
        string region)
    {
        return new EntidadAcademicaDatosNormalizados(
            clave.Trim(),
            nombre.Trim(),
            calleNumero.Trim(),
            colonia.Trim(),
            cp.Trim(),
            municipio.Trim(),
            telefono.Trim(),
            extension.Trim(),
            idAreaAcademica,
            region.Trim());
    }

    private static EntidadAcademicaValidacion? ValidarTextoObligatorio(
        string? valor,
        int longitudMaxima,
        string campo,
        string mensajeObligatorio,
        string mensajeLongitud)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return new EntidadAcademicaValidacion(campo, mensajeObligatorio);
        }

        return ValidarLongitud(valor, longitudMaxima, campo, mensajeLongitud);
    }

    private static EntidadAcademicaValidacion? ValidarLongitud(
        string valor,
        int longitudMaxima,
        string campo,
        string mensaje)
    {
        return valor.Trim().Length > longitudMaxima
            ? new EntidadAcademicaValidacion(campo, mensaje)
            : null;
    }
}
