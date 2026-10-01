using Microsoft.AspNetCore.StaticFiles;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SGPla.Commons;
using SGPla.Models;
using SGPla.Models.DTOs.Archivo;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using System.Diagnostics;
using V = DocumentFormat.OpenXml.Vml;

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

        public async Task<ArchivoDescargadoDTO> ObtenerVistaPreviaPdfAsync(int idArchivo, bool agregarMarcaAguaInformativa = false)
        {
            var documento = await DescargarAsync(idArchivo);
            var extension = Path.GetExtension(documento.Ruta);
            if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                documento.Tipo = "application/pdf";
                return documento;
            }

            if (!extension.Equals(".docx", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Sólo se admiten documentos DOCX o PDF.");

            var directorioVistasPrevias = Path.Combine(_rutaBase, "aviso-preview");
            var sufijo = agregarMarcaAguaInformativa ? "-informativo" : string.Empty;
            var rutaPdf = Path.Combine(directorioVistasPrevias, $"{idArchivo}{sufijo}.pdf");
            if (!File.Exists(rutaPdf))
            {
                Directory.CreateDirectory(directorioVistasPrevias);

                var perfilTemporal = Path.Combine(Path.GetTempPath(), $"sgpla-libreoffice-{Guid.NewGuid():N}");
                Directory.CreateDirectory(perfilTemporal);
                try
                {
                    var rutaDocumentoConversion = documento.Ruta;
                    if (agregarMarcaAguaInformativa)
                    {
                        rutaDocumentoConversion = Path.Combine(perfilTemporal, $"aviso-{Guid.NewGuid():N}.docx");
                        File.Copy(documento.Ruta, rutaDocumentoConversion);
                        agregarMarcaAgua(rutaDocumentoConversion);
                    }

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
                    inicio.ArgumentList.Add(rutaDocumentoConversion);

                    using var proceso = Process.Start(inicio)
                        ?? throw new InvalidOperationException("No se pudo iniciar LibreOffice para generar la vista previa.");
                    var salidaError = await proceso.StandardError.ReadToEndAsync();
                    await proceso.WaitForExitAsync();

                    var rutaPdfGenerada = Path.Combine(
                        directorioVistasPrevias,
                        $"{Path.GetFileNameWithoutExtension(rutaDocumentoConversion)}.pdf");
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
                Nombre = agregarMarcaAguaInformativa
                    ? $"{Path.GetFileNameWithoutExtension(documento.Nombre)}-informativo.pdf"
                    : $"{Path.GetFileNameWithoutExtension(documento.Nombre)}.pdf",
                Tipo = "application/pdf"
            };
        }

        private static void agregarMarcaAgua(string rutaDocumento)
        {
            const string textoMarcaAgua =
                "DOCUMENTO DE CARÁCTER INFORMATIVO - DEBE ENVIARSE PARA SU REVISIÓN Y APROBACIÓN";

            using var documento = WordprocessingDocument.Open(rutaDocumento, true);
            var partePrincipal = documento.MainDocumentPart
                ?? throw new InvalidDataException("El documento no contiene una parte principal válida.");

            var encabezados = partePrincipal.HeaderParts.ToList();
            if (encabezados.Count == 0)
            {
                var encabezado = partePrincipal.AddNewPart<HeaderPart>();
                encabezado.Header = new Header();
                encabezados.Add(encabezado);

                foreach (var seccion in partePrincipal.Document.Body?.Descendants<SectionProperties>() ?? [])
                {
                    seccion.RemoveAllChildren<HeaderReference>();
                    seccion.PrependChild(new HeaderReference
                    {
                        Type = HeaderFooterValues.Default,
                        Id = partePrincipal.GetIdOfPart(encabezado)
                    });
                }
            }

            foreach (var encabezado in encabezados)
            {
                encabezado.Header ??= new Header();
                encabezado.Header.Append(crearMarcaAgua(textoMarcaAgua));
                encabezado.Header.Save();
            }

            partePrincipal.Document.Save();
        }

        private static Paragraph crearMarcaAgua(string texto)
        {
            var lineas = texto.Split(" - ", 2, StringSplitOptions.None);
            var tipoForma = new V.Shapetype
            {
                Id = "_x0000_t136",
                CoordinateSize = "21600,21600",
                OptionalNumber = 136,
                Adjustment = "10800",
                EdgePath = "m@7,l@8,m@5,21600l@6,21600e"
            };
            tipoForma.Append(
                new V.Formulas(
                    new V.Formula { Equation = "sum #0 0 10800" },
                    new V.Formula { Equation = "prod #0 2 1" },
                    new V.Formula { Equation = "sum 21600 0 @1" },
                    new V.Formula { Equation = "sum 0 0 @2" },
                    new V.Formula { Equation = "sum 21600 0 @3" },
                    new V.Formula { Equation = "if @0 @3 0" },
                    new V.Formula { Equation = "if @0 21600 @1" },
                    new V.Formula { Equation = "if @0 0 @2" },
                    new V.Formula { Equation = "if @0 @4 21600" },
                    new V.Formula { Equation = "mid @5 @6" },
                    new V.Formula { Equation = "mid @8 @5" },
                    new V.Formula { Equation = "mid @7 @8" },
                    new V.Formula { Equation = "mid @6 @7" },
                    new V.Formula { Equation = "sum @6 0 @5" }),
                new V.Path { AllowTextPath = true },
                new V.TextPath { On = true, FitShape = true },
                new V.ShapeHandles(
                    new V.ShapeHandle { Position = "#0,bottomRight", XRange = "6629,14971" }));

            return new Paragraph(
                new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                new Run(
                    new Picture(
                        tipoForma,
                        crearLineaMarcaAgua(lineas[0], -34),
                        crearLineaMarcaAgua(lineas.Length > 1 ? lineas[1] : string.Empty, 34))));
        }

        private static V.Shape crearLineaMarcaAgua(string texto, int desplazamientoVertical)
        {
            var forma = new V.Shape
            {
                Id = $"MarcaAguaInformativa_{Guid.NewGuid():N}",
                Type = "#_x0000_t136",
                Style = $"position:absolute;margin-left:0;margin-top:{desplazamientoVertical}pt;" +
                        "width:560pt;height:70pt;rotation:315;z-index:-251654144;" +
                        "mso-position-horizontal:center;mso-position-horizontal-relative:margin;" +
                        "mso-position-vertical:center;mso-position-vertical-relative:margin",
                FillColor = "#8F8F8F",
                Stroked = false
            };
            forma.Append(
                new V.Fill { Opacity = ".55" },
                new V.TextPath
                {
                    Style = "font-family:Arial;font-size:1pt;font-weight:bold",
                    String = texto
                });

            return forma;
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
