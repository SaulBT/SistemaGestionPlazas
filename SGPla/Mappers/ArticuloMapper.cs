using SGPla.Models;
using SGPla.Models.DTOs.Articulo;

namespace SGPla.Mappers
{
    public class ArticuloMapper
    {
        public static Articulo ToModel(FormularioArticuloDTO articuloDTO)
        {
            return new Articulo
            {
                Numero = articuloDTO.Numero,
                Descripcion = articuloDTO.Descripcion
            };
        }

        public static ArticuloDTO ToDTO(Articulo articulo)
        {
            return new ArticuloDTO
            {
                IdArticulo = articulo.IdArticulo ?? 0,
                Numero = articulo.Numero,
                Descripcion = articulo.Descripcion
            };
        }

        public static FormularioArticuloDTO ToFormularioDTO(Articulo articulo)
        {
            return new FormularioArticuloDTO
            {
                IdArticulo = articulo.IdArticulo ?? 0,
                Numero = articulo.Numero,
                Descripcion = articulo.Descripcion
            };
        }

    }
}
