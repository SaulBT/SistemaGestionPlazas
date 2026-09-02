using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.ViewModels.Articulos
{
    public class ArticuloFormularioViewModel
    {
        public int? IdArticulo { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        public string? Numero { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string? Descripcion { get; set; }

    }
}
