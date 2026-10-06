namespace SGPla.Models.ViewModels.Avisos
{
    public class VistaPreviaAvisoViewModel
    {
        public string UrlVistaPrevia { get; set; } = string.Empty;
        public string UrlDescarga { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public bool EsPdf { get; set; }
        public bool Imprimir { get; set; }
    }
}
