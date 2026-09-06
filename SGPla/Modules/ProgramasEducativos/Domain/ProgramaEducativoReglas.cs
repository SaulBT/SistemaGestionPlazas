using SGPla.Commons;

namespace SGPla.Modules.ProgramasEducativos.Domain;

public sealed record ProgramaEducativoValidacion(
    string Campo,
    string Mensaje);

public sealed record ProgramaEducativoDatosNormalizados(
    string Nombre,
    string Campus,
    int IdEntidadAcademica);

public static class ProgramaEducativoReglas
{
    public const int LONGITUD_MAXIMA_NOMBRE = 100;
    public const int LONGITUD_MAXIMA_CAMPUS = 100;
    public const int CANTIDAD_MAXIMA = 100;

    public static ProgramaEducativoValidacion? ValidarConsulta(
        string? region,
        int? idAreaAcademica,
        int? idEntidadAcademica,
        int pagina,
        int cantidad)
    {
        if (!string.IsNullOrWhiteSpace(region)
            && !Constantes.REGIONES.Contains(region.Trim()))
        {
            return new ProgramaEducativoValidacion(
                "region",
                "La región indicada no es válida.");
        }

        if (idAreaAcademica.HasValue && idAreaAcademica <= 0)
        {
            return new ProgramaEducativoValidacion(
                "idAreaAcademica",
                "El ID del área académica no es válido.");
        }

        if (idEntidadAcademica.HasValue && idEntidadAcademica <= 0)
        {
            return new ProgramaEducativoValidacion(
                "idEntidadAcademica",
                "El ID de la entidad académica no es válido.");
        }

        if (pagina <= 0)
        {
            return new ProgramaEducativoValidacion(
                "pagina",
                "La página debe ser mayor que cero.");
        }

        if (cantidad <= 0 || cantidad > CANTIDAD_MAXIMA)
        {
            return new ProgramaEducativoValidacion(
                "cantidad",
                "La cantidad debe estar entre 1 y 100.");
        }

        return null;
    }

    public static ProgramaEducativoValidacion? ValidarDatos(
        string? nombre,
        string? campus,
        int? idEntidadAcademica)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return new ProgramaEducativoValidacion(
                "nombre",
                "El nombre del programa educativo es obligatorio.");
        }

        if (nombre.Trim().Length > LONGITUD_MAXIMA_NOMBRE)
        {
            return new ProgramaEducativoValidacion(
                "nombre",
                "El nombre del programa educativo no puede exceder los 100 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(campus))
        {
            return new ProgramaEducativoValidacion(
                "campus",
                "El campus es obligatorio.");
        }

        if (campus.Trim().Length > LONGITUD_MAXIMA_CAMPUS)
        {
            return new ProgramaEducativoValidacion(
                "campus",
                "El campus no puede exceder los 100 caracteres.");
        }

        if (!idEntidadAcademica.HasValue || idEntidadAcademica <= 0)
        {
            return new ProgramaEducativoValidacion(
                "idEntidadAcademica",
                "El ID de la entidad académica es obligatorio y debe ser válido.");
        }

        return null;
    }

    public static ProgramaEducativoDatosNormalizados Normalizar(
        string nombre,
        string campus,
        int idEntidadAcademica)
    {
        return new ProgramaEducativoDatosNormalizados(
            nombre.Trim(),
            campus.Trim(),
            idEntidadAcademica);
    }

    public static string ObtenerClave(string nombre)
    {
        var separador = nombre.IndexOf('-');
        return (separador < 0 ? nombre : nombre[..separador]).Trim();
    }
}
