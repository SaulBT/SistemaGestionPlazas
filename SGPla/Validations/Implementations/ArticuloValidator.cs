using SGPla.Models.DTOs.Articulo;
using SGPla.Models.InterfacesDTOs;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Interfaces;
using System.Text.RegularExpressions;

namespace SGPla.Validations.Implementations
{
    public class ArticuloValidator : IArticuloValidator
    {
        private readonly IArticuloRepository _articuloRepository;

        public ArticuloValidator(IArticuloRepository articuloRepository)
        {
            _articuloRepository = articuloRepository;
        }

        public async Task ValidarCreacionAsync(CrearArticuloDTO crearArticuloDTO)
        {
            ArgumentNullException.ThrowIfNull(crearArticuloDTO);

            ValidarCampos(crearArticuloDTO);
            await ValidarNoRepetidoCreacionAsync(crearArticuloDTO.Numero);
        }

        public async Task ValidarEdicionAsync(EditarArticuloDTO editarArticuloDTO)
        {
            ArgumentNullException.ThrowIfNull(editarArticuloDTO);

            ValidarCampos(editarArticuloDTO);
            await ValidarNoRepetidoEdicionAsync(editarArticuloDTO.Numero, editarArticuloDTO.IdArticulo);
        }


        private static void ValidarCampos(IArticuloDTO articuloDTO)
        {
            if (string.IsNullOrWhiteSpace(articuloDTO.Numero))
                throw new ArgumentException("El número del artículo es obligatorio.");

            if (!Regex.IsMatch(articuloDTO.Numero, @"\d"))
                throw new ArgumentException("El número del artículo debe contener al menos un número.");

            if (string.IsNullOrWhiteSpace(articuloDTO.Descripcion))
                throw new ArgumentException("La descripción del artículo es obligatoria.");

           
        }

        private async Task ValidarNoRepetidoCreacionAsync(string numero)
        {
            var existeNumero = await _articuloRepository.ExisteNumeroAsync(numero);

            if (existeNumero is not null)
            {
                throw new ArgumentException($"El número de articulo '{numero}' ya existe. Por favor, elija un número diferente.");
            }

        }

        private async Task ValidarNoRepetidoEdicionAsync(string numero, int idArticulo)
        {
            var existeNumero = await _articuloRepository.ExisteNumeroAsync(numero);

            if (existeNumero is not null && existeNumero.IdArticulo != idArticulo)
            {
                throw new ArgumentException($"El número de articulo '{numero}' ya existe. Por favor, elija un número diferente.");
            }

        }

        public async Task ValidarBusquedaPorTerminoAsync(string busqueda)
        {
            if (string.IsNullOrWhiteSpace(busqueda))
                throw new ArgumentException("La cadena de búsqueda no puede estar vacía.");
        }

        public async Task ValidarObtenerPorIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID del artículo no es válido.");
        }

        public async Task ValidarEliminarAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID del artículo no es válido.");
        }
    }
}
