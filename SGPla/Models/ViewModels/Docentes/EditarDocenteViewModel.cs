using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.Docentes
{
    public class EditarDocenteViewModel
    {
        public int IdDocente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string DescripcionPerfil { get; set; } = string.Empty;
        public IFormFile Archivo { get; set; }
        public FormularioGradoViewModel Formluario { get; set; } = new FormularioGradoViewModel();
        public TableModel Tabla { get; set; } = new TableModel();
        public string NumeroPersonal { get; set; } = string.Empty;
        public List<OptionModel> OpcionesPuesto { get; set; } = [];
        public string Puesto { get; set; } = string.Empty;
        public bool NuevoArchivo { get; set; } = false;
        public bool Recarga { get; set; } = false;
    }
}
