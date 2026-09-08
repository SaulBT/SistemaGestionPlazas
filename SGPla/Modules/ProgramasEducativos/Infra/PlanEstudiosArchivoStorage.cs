using SGPla.Modules.ProgramasEducativos.Application.Models;
using SGPla.Modules.ProgramasEducativos.Application.Ports;
using SGPla.Modules.ProgramasEducativos.Domain;

namespace SGPla.Modules.ProgramasEducativos.Infra;

public sealed class PlanEstudiosArchivoStorage : IPlanEstudiosArchivoStorage
{
    private readonly string _rutaBase;

    public PlanEstudiosArchivoStorage(IConfiguration configuration, IHostEnvironment environment)
    {
        var rutaConfigurada = configuration["Archivos:RutaBase"]
            ?? throw new InvalidOperationException(
                "No se encontró la configuración Archivos:RutaBase.");

        _rutaBase = Path.IsPathRooted(rutaConfigurada)
            ? rutaConfigurada
            : Path.GetFullPath(rutaConfigurada, environment.ContentRootPath);
    }

    public async Task<ArchivoPlanGuardado> GuardarAsync(
        ArchivoPlanParaGuardar archivo,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(archivo.NombreOriginal).ToLowerInvariant();
        var nombreGuardado = $"{Guid.NewGuid():N}{extension}";
        var ruta = Path.Combine(PlanEstudiosConstantes.CarpetaArchivos, nombreGuardado)
            .Replace("\\", "/");
        var rutaFisica = ObtenerRutaSegura(ruta);
        var directorio = Path.GetDirectoryName(rutaFisica)
            ?? throw new InvalidOperationException(
                "No fue posible determinar el directorio del archivo del plan.");

        Directory.CreateDirectory(directorio);

        try
        {
            await File.WriteAllBytesAsync(rutaFisica, archivo.Contenido, cancellationToken);
        }
        catch
        {
            if (File.Exists(rutaFisica))
            {
                File.Delete(rutaFisica);
            }

            throw;
        }

        return new ArchivoPlanGuardado(
            Path.GetFileName(archivo.NombreOriginal),
            ruta,
            string.IsNullOrWhiteSpace(archivo.Tipo)
                ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                : archivo.Tipo,
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
