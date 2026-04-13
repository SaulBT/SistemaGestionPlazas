using SGPla.Models;
using SGPla.Models.DTOs.Articulo;

namespace SGPla.Mappers
{
    public class ArticuloMapper
    {
        public static Articulo ToModel(CrearArticuloDTO dto)
        {
            return new Articulo
            {
                Numero = dto.Numero,
                Descripcion = dto.Descripcion
            };
        }

        public static Articulo ToModel(EditarArticuloDTO dto)
        {
            return new Articulo
            {
                IdArticulo = dto.IdArticulo,
                Numero = dto.Numero,
                Descripcion = dto.Descripcion
            };

        }

        public static DetallesArticuloDTO ToDTO(Articulo articulo)
        {
            return new DetallesArticuloDTO
            {
                IdArticulo = articulo.IdArticulo,
                Numero = articulo.Numero,
                Descripcion = articulo.Descripcion
            };
        }

    }
}
