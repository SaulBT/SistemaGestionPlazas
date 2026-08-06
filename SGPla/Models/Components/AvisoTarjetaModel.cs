namespace SGPla.Models.Components
{
    public class AvisoTarjetaModel
    {
        public int IdAviso { get; set; }
        public string EntidadAcademica { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;
        public string Periodo { get; set; } = string.Empty;
        public string Articulo { get; set; } = string.Empty;
        public string FechaCreacion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Comentarios { get; set; } = string.Empty;
        public bool Archivado { get; set; } = false;
    }
}
