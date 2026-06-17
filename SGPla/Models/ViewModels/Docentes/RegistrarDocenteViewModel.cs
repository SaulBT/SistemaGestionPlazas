namespace SGPla.Models.ViewModels.Docentes
{
    public class RegistrarDocenteViewModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string DescripcionPerfil { get; set; } = string.Empty;
        public IFormFile Archivo { get; set; }
        public TableModel Tabla { get; set; } = new TableModel();
        public string NumeroPersonal { get; set; } = string.Empty;
        public string Puesto { get; set; } = string.Empty;
        public bool Recarga { get; set; } = false;
    }
}
