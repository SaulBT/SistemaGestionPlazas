using SGPla.Models;
using SGPla.Models.DTOs.Articulo;

namespace SGPla.Mappers
{
    public class ArticuloMapper
    {
        public static Articulo ToModel(CrearArticuloDTO dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto), "El DTO de creación de artículo proporcionó datos nulos.");

            return new Articulo
            {
                Numero = dto.Numero,
                Descripcion = dto.Descripcion
            };
        }

        public static Articulo ToModel(EditarArticuloDTO dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto), "El DTO de creación de artículo proporcionó datos nulos.");

            return new Articulo
            {
                IdArticulo = dto.IdArticulo,
                Numero = dto.Numero,
                Descripcion = dto.Descripcion
            };

        }

        public static DetallesArticuloDTO ToDTO(Articulo articulo)
        {
            if (articulo is null)
                throw new ArgumentNullException(nameof(articulo), "El artículo proporcionó datos nulos.");

            return new DetallesArticuloDTO
            {
                IdArticulo = articulo.IdArticulo,
                Numero = articulo.Numero,
                Descripcion = articulo.Descripcion
            };
        }

    }
}
