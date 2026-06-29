namespace SGPla.Models.DTOs.Aviso
{
    public class RevisionDTO
    {
        public int IdAviso { get; set; }
        public bool Aprobado { get; set; }
        public string Comentarios { get; set; } = string.Empty;
    }
}
