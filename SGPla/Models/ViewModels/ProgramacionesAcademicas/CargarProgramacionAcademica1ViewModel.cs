using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.ProgramacionesAcademicas
{
    public class CargarProgramacionAcademica1ViewModel
    {
        public List<OptionModel> Regiones { get; set; }

        public List<OptionModel> Periodos { get; set; }
        public List<OptionModel> Entidades { get; set; }

        public string? Region { get; set; }

        public int? IdEntidadAcademica { get; set; }

        public int? IdPeriodo { get; set; }

        public IFormFile? ArchivoVacantes { get; set; }
        public IFormFile? ArchivoDescargas { get; set; }

    }
}
