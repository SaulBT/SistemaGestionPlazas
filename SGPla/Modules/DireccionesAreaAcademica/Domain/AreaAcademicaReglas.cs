namespace SGPla.Modules.DireccionesAreaAcademica.Domain;

public sealed record AreaAcademicaValidacion(
    string Campo,
    string Mensaje);

public static class AreaAcademicaReglas
{
    public const int NOMBRE_LONGITUD_MAXIMA = 100;
    public const int TELEFONO_LONGITUD_MAXIMA = 30;
    public const int EXTENSION_LONGITUD_MAXIMA = 5;
    public const int PAGINA_POR_DEFECTO = 1;
    public const int CANTIDAD_POR_DEFECTO = 10;
    public const int CANTIDAD_MAXIMA = 100;

    public static AreaAcademicaValidacion? ValidarDatos(
        string? nombre,
        string? telefono,
        string? extension)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return new AreaAcademicaValidacion(
                "nombre",
                "El nombre es obligatorio.");
        }

        if (nombre.Trim().Length > NOMBRE_LONGITUD_MAXIMA)
        {
            return new AreaAcademicaValidacion(
                "nombre",
                "El nombre no puede exceder los 100 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(telefono))
        {
            return new AreaAcademicaValidacion(
                "telefono",
                "El teléfono es obligatorio.");
        }

        if (telefono.Trim().Length > TELEFONO_LONGITUD_MAXIMA)
        {
            return new AreaAcademicaValidacion(
                "telefono",
                "El teléfono no puede exceder los 30 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(extension))
        {
            return new AreaAcademicaValidacion(
                "extension",
                "La extensión es obligatoria.");
        }

        if (extension.Trim().Length > EXTENSION_LONGITUD_MAXIMA)
        {
            return new AreaAcademicaValidacion(
                "extension",
                "La extensión no puede exceder los 5 caracteres.");
        }

        return null;
    }

    public static AreaAcademicaValidacion? ValidarConsulta(
        int pagina,
        int cantidad)
    {
        if (pagina <= 0)
        {
            return new AreaAcademicaValidacion(
                "pagina",
                "La página debe ser mayor que cero.");
        }

        if (cantidad <= 0 || cantidad > CANTIDAD_MAXIMA)
        {
            return new AreaAcademicaValidacion(
                "cantidad",
                "La cantidad debe estar entre 1 y 100.");
        }

        return null;
    }

    public static (string Nombre, string Telefono, string Extension) Normalizar(
        string nombre,
        string telefono,
        string extension)
    {
        return (nombre.Trim(), telefono.Trim(), extension.Trim());
    }
}
