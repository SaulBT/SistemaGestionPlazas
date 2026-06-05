using Microsoft.AspNetCore.StaticFiles;
using SGPla.Commons;
using SGPla.Models;
using SGPla.Models.DTOs.Archivo;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations
{
    public class ArchivoService : IArchivoService
    {
        private readonly string _rutaBase;
        private readonly FileExtensionContentTypeProvider _contentTypeProvider;
        private readonly IArchivoRepository _archivoRepository;

        public ArchivoService(IConfiguration configuration, IHostEnvironment environment, IArchivoRepository archivoRepository)
        {
            var rutaConfigurada = configuration["Archivos:RutaBase"]
                ?? throw new InvalidOperationException("No se encontró la configuración Archivos:RutaBase.");

            _rutaBase = Path.IsPathRooted(rutaConfigurada)
            ? rutaConfigurada
            : Path.GetFullPath(rutaConfigurada, environment.ContentRootPath);

            _contentTypeProvider = new FileExtensionContentTypeProvider();
            _archivoRepository = archivoRepository;
        }

        public async Task<DatosArchivoGuardadoDTO> GuardarAsync(string rutaOrigen, string nombreOriginal, string carpeta)
        {
            if (string.IsNullOrWhiteSpace(rutaOrigen))
                throw new ArgumentException("La ruta del archivo es obligatoria.");

            if (string.IsNullOrWhiteSpace(nombreOriginal))
                throw new ArgumentException("El nombre del archivo es obligatorio.");

            if (string.IsNullOrWhiteSpace(carpeta))
                throw new ArgumentException("La carpeta de destino es obligatoria.");

            var extension = Path.GetExtension(nombreOriginal);
            var nombreGuardado = $"{Guid.NewGuid()}{extension}";
            var carpetaFisica = Path.Combine(_rutaBase, carpeta);
            var rutaFisica = Path.Combine(carpetaFisica, nombreGuardado);

            Directory.CreateDirectory(carpetaFisica);

            using (var origen = new FileStream(rutaOrigen, FileMode.Open, FileAccess.Read))
            {
                using (var destino = new FileStream(rutaFisica, FileMode.Create, FileAccess.Write, FileShare.None))
                    await origen.CopyToAsync(destino);
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

        public async Task<(string nombre, string ruta)> GuardarTemporalmenteAsync(IFormFile archivo)
        {
            var carpetaTemp = Path.Combine(_rutaBase, "temp-uploads");
            Directory.CreateDirectory(carpetaTemp);
            var extension = Path.GetExtension(archivo.FileName);
            var rutaArchivo = Path.Combine(carpetaTemp, $"{Guid.NewGuid()}{extension}");
            using var stream = new FileStream(rutaArchivo, FileMode.Create);
            await archivo.CopyToAsync(stream);

            return (archivo.FileName, rutaArchivo);
        }

        public async Task<ArchivoDescargadoDTO> DescargarAsync(int idArchivo)
        {
            if (idArchivo <= 0)
                throw new ArgumentException("La IdArchivo es inválida.");

            var archivo = await _archivoRepository.ObtenerPorIdAsync(idArchivo);

            if (archivo == null)
                throw new KeyNotFoundException("No existe ese Archivo.");

            var rutaFisica = Path.Combine(_rutaBase, archivo.Ruta);

            if (!File.Exists(rutaFisica))
                throw new FileNotFoundException("No se encontró el archivo físico.", archivo.Ruta);

            return new ArchivoDescargadoDTO
            {
                Ruta = rutaFisica,
                Nombre = archivo.Nombre,
                Tipo = archivo.Tipo
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