namespace SGPla.Models.DTOs.PlanEstudios
{
    public class ArchivoPlanEstudiosDTO
    {
        public Stream Archivo { get; set; } = new MemoryStream();
        public string NombreArchivo { get; set; } = string.Empty;
    }
}
