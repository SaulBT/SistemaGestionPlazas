namespace SGPla.Models.DTOs.Docentes
{
    public class ListaAspiranteDTO
    {
        public int IdDocente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string UltimoGrado { get; set; } = string.Empty;
        public string DescripcionPerfil { get; set; } = string.Empty;
        public int IdArchivosGenerales { get; set; }
    }
}
