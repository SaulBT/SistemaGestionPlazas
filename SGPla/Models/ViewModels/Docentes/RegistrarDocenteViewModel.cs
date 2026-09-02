using SGPla.Models.Components;
using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.ViewModels.Docentes
{
    public class RegistrarDocenteViewModel
    {
        [Required(ErrorMessage = "El Nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;
        [Required(ErrorMessage = "La Descripción es obligatoria.")]
        public string DescripcionPerfil { get; set; } = string.Empty;
        public IFormFile Archivo { get; set; }
        public FormularioGradoViewModel Formulario { get; set; } = new FormularioGradoViewModel();
        public TableModel Tabla { get; set; } = new TableModel();
        [Required(ErrorMessage = "El Numero personal es obligatorio.")]
        public string NumeroPersonal { get; set; } = string.Empty;
        public List<OptionModel> OpcionesPuesto { get; set; } = [];
        [Required(ErrorMessage = "El Puesto es obligatorio.")]
        public string Puesto { get; set; } = string.Empty;
        public bool Recarga { get; set; } = false;
    }
}
