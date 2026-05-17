namespace SGPla.Models.ViewModels.PlanesEstudios
{
    public class EditarPlanViewModel
    {
        public int IdPlanEstudios {  get; set; }
        public string Area { get; set; } = string.Empty;
        public string Programa { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string Sistema { get; set; } = string.Empty;
        public FormularioExperienciaEducativaViewModel Formulario { get; set; }
        public TableModel Table { get; set; }
        public IFormFile Archivo { get; set; }
        public bool NuevoArchivo { get; set; } = false;
    }
}
