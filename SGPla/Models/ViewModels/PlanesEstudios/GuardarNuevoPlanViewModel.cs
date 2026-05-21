namespace SGPla.Models.ViewModels.PlanesEstudios
{
    public class GuardarNuevoPlanViewModel
    {
        public int IdProgramaEducativo { get; set; }
        public string Plan { get; set; } = string.Empty;
        public string Sistema { get; set; } = string.Empty;

        public string NombrePrograma { get; set; } = string.Empty;
        public string NombreArea { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }
}
