using SGPla.Models.DTOs.Archivo;

namespace SGPla.Models.DTOs.Aviso
{
    // El aviso firmado se entrega como PDF; los demás estados se muestran como HTML imprimible.
    public class DocumentoAvisoDTO
    {
        public ArchivoDescargadoDTO? Pdf { get; set; }
        public string ContenidoHtml { get; set; } = string.Empty;
    }
}
