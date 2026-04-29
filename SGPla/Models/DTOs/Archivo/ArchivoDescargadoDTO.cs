namespace SGPla.Models.DTOs.Archivo
{
    public class ArchivoDescargadoDTO
    {
        public Stream Contenido { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Tipo { get; set; } = null!;
    }
}
