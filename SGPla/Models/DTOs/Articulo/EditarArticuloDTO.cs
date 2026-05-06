using SGPla.Models.InterfacesDTOs;
using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.DTOs.Articulo
{
    public class EditarArticuloDTO : IArticuloDTO
    {

        public int IdArticulo { get; set; } = 0;
        [Required(ErrorMessage = "El número es obligatorio")]
        public string? Numero { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string? Descripcion { get; set; } = string.Empty;
    }
}
