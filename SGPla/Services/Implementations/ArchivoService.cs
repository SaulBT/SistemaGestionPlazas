using Microsoft.AspNetCore.StaticFiles;
using SGPla.Models.DTOs.Archivo;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations
{
    public class ArchivoService : IArchivoService
    {
        private readonly string _rutaBase;
        private readonly FileExtensionContentTypeProvider _contentTypeProvider;

        public ArchivoService(IConfiguration configuration, IHostEnvironment environment)
        {
            var rutaConfigurada = configuration["Archivos:RutaBase"]
                ?? throw new InvalidOperationException("No se encontró la configuración Archivos:RutaBase.");

            _rutaBase = Path.IsPathRooted(rutaConfigurada)
            ? rutaConfigurada
            : Path.GetFullPath(rutaConfigurada, environment.ContentRootPath);

            _contentTypeProvider = new FileExtensionContentTypeProvider();
        }

        public async Task<DatosArchivoGuardadoDTO> GuardarAsync(Stream archivo, string nombreOriginal, string carpeta)
        {
            ArgumentNullException.ThrowIfNull(archivo);

            if (string.IsNullOrWhiteSpace(nombreOriginal))
                throw new ArgumentException("El nombre del archivo es obligatorio.");

            var extension = Path.GetExtension(nombreOriginal);
            var nombreGuardado = $"{Guid.NewGuid()}{extension}";
            var carpetaFisica = Path.Combine(_rutaBase, carpeta);
            var rutaFisica = Path.Combine(carpetaFisica, nombreGuardado);

            Directory.CreateDirectory(carpetaFisica);

            if (archivo.CanSeek)
                archivo.Position = 0;

            using (var fileStream = new FileStream(rutaFisica, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await archivo.CopyToAsync(fileStream);
            }

            var rutaRelativa = Path.Combine(carpeta, nombreGuardado).Replace("\\", "/");

            if (!_contentTypeProvider.TryGetContentType(nombreOriginal, out var tipo))
                tipo = "application/octet-stream";

            var tamanio = new FileInfo(rutaFisica).Length;

            return new DatosArchivoGuardadoDTO
            {
                NombreOriginal = nombreOriginal,
                Ruta = rutaRelativa,
                Tipo = tipo,
                Tamanio = tamanio
            };
        }

        public Task EliminarAsync(string rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa))
                return Task.CompletedTask;

            var rutaFisica = Path.Combine(_rutaBase, rutaRelativa);

            if (File.Exists(rutaFisica))
                File.Delete(rutaFisica);

            return Task.CompletedTask;
        }
    }
}