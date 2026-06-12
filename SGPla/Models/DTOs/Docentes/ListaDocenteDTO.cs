namespace SGPla.Models.DTOs.Docentes
{
    public class ListaDocenteDTO
    {
        public int IdDocente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string NumeroPersonal { get; set; } = string.Empty;
        public string Puesto { get; set; } = string.Empty;
        public int IdArchivosGenerales { get; set; }
    }
}