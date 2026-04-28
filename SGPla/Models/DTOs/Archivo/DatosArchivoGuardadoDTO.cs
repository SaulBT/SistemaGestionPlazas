namespace SGPla.Models.DTOs.Archivo
{
    public class DatosArchivoGuardadoDTO
    {
        public string NombreOriginal { get; set; } = null!;
        public string Ruta { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public long Tamanio { get; set; }
    }
}
