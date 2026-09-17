using SGPla.Models.Components;
using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.ViewModels.ProgramacionesAcademicas
{
    public class CargarProgramacionAcademica1ViewModel
    {
        public List<OptionModel>? Regiones { get; set; }

        public List<OptionModel>? Periodos { get; set; }
        public List<OptionModel>? Entidades { get; set; }
        public List<OptionModel>? Modalidades { get; set; }
        public List<OptionModel>? Planes { get; set; }


        [Required(ErrorMessage = "La región es requerida.")]
        public string? Region { get; set; }

        [Required(ErrorMessage = "La entidad académica es requerida.")]
        public int? IdEntidadAcademica { get; set; }

        [Required(ErrorMessage = "El periodo es requerido.")]
        public int? IdPeriodo { get; set; }

        [Required(ErrorMessage = "La modalidad es requerida.")]
        public string? Modalidad { get; set; }

        [Required(ErrorMessage = "El plan de estudios es requerido.")]
        public int? IdPlanEstudios { get; set; }

        [Required(ErrorMessage = "Seleccione un archivo de vacantes.")]
        public IFormFile? ArchivoVacantes { get; set; }

        [Required(ErrorMessage = "Seleccione un archivo de descargas.")]
        public IFormFile? ArchivoDescargas { get; set; }

        [Required(ErrorMessage = "Seleccione un archivo de cargas.")]
        public IFormFile? ArchivoCargas { get; set; }

    }
}
