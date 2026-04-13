using SGPla.Models.InterfacesDTOs;

namespace SGPla.Models.DTOs.Articulo
{
    public class CrearArticuloDTO : IArticuloDTO
    {
        public string Numero { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

    }
}
