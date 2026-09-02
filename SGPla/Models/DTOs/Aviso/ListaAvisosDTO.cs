namespace SGPla.Models.DTOs.Aviso
{
    public class ListaAvisosDTO
    {
        public int IdAviso { get; set; }
        public int IdEntidadAcademica { get; set; }
        public int IdPeriodo { get; set; }
        public int IdArticulo { get; set; }
        public string NombreEntidadAcademica { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;
        public string Periodo { get; set; } = string.Empty;
        public string Articulo { get; set; } = string.Empty;
        public string FechaCreacion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool Archivado { get; set; } = false;
        public string Comentarios { get; set; } = string.Empty;
    }
}
