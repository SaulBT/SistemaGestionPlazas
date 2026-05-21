using SGPla.Models.DTOs.PlanEstudios;

namespace SGPla.Models.ViewModels.PlanesEstudios
{
    public class VerPlanEstudiosViewModel
    {
        public int IdPlanEstudios { get; set; }
        public string NombreProgramaEducativo { get; set; } = string.Empty;
        public string Modalidad { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string NombreAreaAcademica { get; set; } = string.Empty;
        public List<DatosExperienciaEducativaDTO> ExperienciasEducativas { get; set; } = [];
        public int IdArchivo { get; set; }
        public TableModel Table { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int CantidadPorPaginas { get; set; } = 10;
    }
}
