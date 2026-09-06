using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Models;
using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Ports;
using SGPla.Modules.SolicitudesApertura.Domain;

namespace SGPla.Modules.SolicitudesApertura.Infra;

public sealed class ArchivoOficioStorage : IArchivoOficioStorage
{
    private readonly string _rutaBase;

    public ArchivoOficioStorage(IConfiguration configuration, IHostEnvironment environment)
    {
        var rutaConfigurada = configuration["Archivos:RutaBase"]
            ?? throw new InvalidOperationException(
                "No se encontró la configuración Archivos:RutaBase.");

        _rutaBase = Path.IsPathRooted(rutaConfigurada)
            ? rutaConfigurada
            : Path.GetFullPath(rutaConfigurada, environment.ContentRootPath);
    }

    public async Task<ArchivoOficioGuardado> GuardarAsync(
        ArchivoOficioParaGuardar archivo,
        CancellationToken cancellationToken)
    {
        var nombreGuardado = $"{Guid.NewGuid():N}{SolicitudAperturaConstantes.EXTENSION_ARCHIVO_OFICIO}";
        var ruta = Path.Combine(
            SolicitudAperturaConstantes.CARPETA_ARCHIVOS_OFICIO,
            nombreGuardado).Replace("\\", "/");
        var rutaFisica = ObtenerRutaSegura(ruta);
        var directorioDestino = Path.GetDirectoryName(rutaFisica)
            ?? throw new InvalidOperationException(
                "No se pudo determinar el directorio del archivo de oficio.");

        Directory.CreateDirectory(directorioDestino);

        try
        {
            await File.WriteAllBytesAsync(
                rutaFisica,
                archivo.Contenido,
                cancellationToken);
        }
        catch
        {
            if (File.Exists(rutaFisica))
            {
                File.Delete(rutaFisica);
            }

            throw;
        }

        return new ArchivoOficioGuardado(
            archivo.NombreOriginal,
            ruta,
            archivo.Tipo,
            archivo.Contenido.LongLength);
    }

    public Task EliminarAsync(string ruta, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rutaFisica = ObtenerRutaSegura(ruta);

        if (File.Exists(rutaFisica))
        {
            File.Delete(rutaFisica);
        }

        return Task.CompletedTask;
    }

    private string ObtenerRutaSegura(string rutaRelativa)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa))
        {
            throw new ArgumentException("La ruta del archivo es obligatoria.", nameof(rutaRelativa));
        }

        var rutaRelativaSistema = rutaRelativa
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);
        var rutaCompleta = Path.GetFullPath(Path.Combine(_rutaBase, rutaRelativaSistema));
        var baseCompleta = Path.GetFullPath(_rutaBase);
        var prefijoBase = Path.TrimEndingDirectorySeparator(baseCompleta)
            + Path.DirectorySeparatorChar;

        if (!rutaCompleta.StartsWith(prefijoBase, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException(
                "Acceso denegado: el archivo está fuera del directorio permitido.");
        }

        return rutaCompleta;
    }
}
