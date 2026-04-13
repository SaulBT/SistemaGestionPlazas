using SGPla.Models.InterfacesDTOs;

namespace SGPla.Models.DTOs.Articulo
{
    public class EditarArticuloDTO : IArticuloDTO
    {

        public int IdArticulo { get; set; } = 0;
        public string Numero { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;
    }
}
