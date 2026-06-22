namespace SGPla.Models.ViewModels.ProgramacionesAcademicas
{
    public class VerProgramacionAcademicaViewModel
    {
        public string? Region { get; set; }
        public string? NombreEntidadAcademica { get; set; }
        public string? NombrePeriodo { get; set; }
        public string? NombrePrograma { get; set; }

        public TableModel TableAsignadas { get; set; } = new();
        public TableModel TableVacantes { get; set; } = new();
    }
}
