using Microsoft.AspNetCore.StaticFiles;
using SGPla.Commons;
using SGPla.Models;
using SGPla.Models.DTOs.Archivo;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using System.Diagnostics;

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

        public async Task<ArchivoDescargadoDTO> ObtenerVistaPreviaPdfAsync(int idArchivo)
        {
            var documento = await DescargarAsync(idArchivo);
            if (!Path.GetExtension(documento.Ruta).Equals(".docx", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Sólo se puede generar una vista previa de documentos DOCX.");

            var directorioVistasPrevias = Path.Combine(_rutaBase, "aviso-preview");
            var rutaPdf = Path.Combine(directorioVistasPrevias, $"{idArchivo}.pdf");
            if (!File.Exists(rutaPdf))
            {
                Directory.CreateDirectory(directorioVistasPrevias);

                var perfilTemporal = Path.Combine(Path.GetTempPath(), $"sgpla-libreoffice-{Guid.NewGuid():N}");
                Directory.CreateDirectory(perfilTemporal);
                try
                {
                    var inicio = new ProcessStartInfo
                    {
                        FileName = "soffice",
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    inicio.ArgumentList.Add($"-env:UserInstallation=file://{perfilTemporal}");
                    inicio.ArgumentList.Add("--headless");
                    inicio.ArgumentList.Add("--convert-to");
                    inicio.ArgumentList.Add("pdf:writer_pdf_Export");
                    inicio.ArgumentList.Add("--outdir");
                    inicio.ArgumentList.Add(directorioVistasPrevias);
                    inicio.ArgumentList.Add(documento.Ruta);

                    using var proceso = Process.Start(inicio)
                        ?? throw new InvalidOperationException("No se pudo iniciar LibreOffice para generar la vista previa.");
                    var salidaError = await proceso.StandardError.ReadToEndAsync();
                    await proceso.WaitForExitAsync();

                    var rutaPdfGenerada = Path.Combine(
                        directorioVistasPrevias,
                        $"{Path.GetFileNameWithoutExtension(documento.Ruta)}.pdf");
                    if (proceso.ExitCode != 0 || !File.Exists(rutaPdfGenerada))
                        throw new InvalidOperationException($"No se pudo convertir el aviso a PDF. {salidaError}");

                    File.Move(rutaPdfGenerada, rutaPdf, true);
                }
                finally
                {
                    Directory.Delete(perfilTemporal, true);
                }
            }

            return new ArchivoDescargadoDTO
            {
                Ruta = rutaPdf,
                Nombre = $"{Path.GetFileNameWithoutExtension(documento.Nombre)}.pdf",
                Tipo = "application/pdf"
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

        public async Task<byte[]> ObtenerArchivoEnBytesAsync(string rutaArchivo)
        {
            string rutaCompleta = Path.GetFullPath(Path.Combine(_rutaBase, rutaArchivo));

            if (!rutaCompleta.StartsWith(_rutaBase, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Acceso denegado a la ruta del archivo.");
            }

            if (!File.Exists(rutaCompleta))
            {
                throw new FileNotFoundException("No se encontró el archivo.", rutaCompleta);
            }

            byte[] archivoBytes = await File.ReadAllBytesAsync(rutaCompleta);

            return archivoBytes;
        }

        public async Task<int> GuardarArchivoBytesAsync(byte[] contenido, string carpeta, string nombre)
        {
            if (contenido == null || contenido.Length == 0)
            {
                throw new ArgumentException("El archivo no contiene datos.", nameof(contenido));
            }

            var nombreGuardado = $"{Guid.NewGuid()}.docx";
            string rutaRelativa = $"{carpeta}/{nombreGuardado}";
            string rutaCompleta = Path.GetFullPath(Path.Combine(_rutaBase, rutaRelativa));

            if (!rutaCompleta.StartsWith(_rutaBase, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Acceso denegado: intento de guardar fuera del directorio permitido.");
            }

            string directorioDestino = Path.GetDirectoryName(rutaCompleta);
            if (!string.IsNullOrEmpty(directorioDestino) && !Directory.Exists(directorioDestino))
            {
                Directory.CreateDirectory(directorioDestino);
            }

            await File.WriteAllBytesAsync(rutaCompleta, contenido);

            if (!_contentTypeProvider.TryGetContentType(nombreGuardado, out var tipo))
                tipo = "application/octet-stream";
            var tamanio = new FileInfo(rutaCompleta).Length;


            var archivo = new Archivo
            {
                Nombre = nombre,
                Tipo = tipo,
                Ruta = rutaRelativa,
                Tamanio = tamanio
            };

            archivo = await _archivoRepository.CrearAsync(archivo);
            return archivo.IdArchivo;
        }
    }
}
