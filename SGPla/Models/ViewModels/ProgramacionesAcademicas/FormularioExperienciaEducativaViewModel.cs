using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.ProgramacionesAcademicas
{
    public class FormularioExperienciaEducativaViewModel
    {
         public string NombreProgramaEducativo { get; set; } = string.Empty;

        public string NombreExperienciaEducativa { get; set; } = string.Empty;

        public string Nrc { get; set; } = string.Empty;

        public string TipoContratacion { get; set; } = string.Empty;

        public int Horas { get; set; }

        public string Modalidad { get; set; } = string.Empty;


        public List<OptionModel> TiposContratacion { get; set; } 


        public TableModel Horarios { get; set; } = new();

        public int IdExperienciaEducativa { get; set; }

        public int IdProgramaEducativo { get; set; }

        public int IdPeriodo { get; set; }

        public int IdEntidadAcademica { get; set; } 



    }
}
