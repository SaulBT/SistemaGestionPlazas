using SGPla.Models.DTOs.Articulo;
using SGPla.Models.InterfacesDTOs;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Validations.Implementations
{
    public class ArticuloValidator : IArticuloValidator
    {
        private readonly IArticuloRepository _articuloRepository;

        public ArticuloValidator(IArticuloRepository articuloRepository)
        {
            _articuloRepository = articuloRepository;
        }

        public async Task<bool> ValidarCreacionAsync(CrearArticuloDTO crearArticuloDTO)
        {
            ArgumentNullException.ThrowIfNull(crearArticuloDTO);

            await ValidarCampos(crearArticuloDTO);
            await ValidarNoRepetidoCreacionAsync(crearArticuloDTO.Numero);
            return true;
        }

        public async Task<bool> ValidarEdicionAsync(EditarArticuloDTO editarArticuloDTO)
        {
            ArgumentNullException.ThrowIfNull(editarArticuloDTO);

            await ValidarCampos(editarArticuloDTO);
            await ValidarNoRepetidoEdicionAsync(editarArticuloDTO.Numero, editarArticuloDTO.IdArticulo);
            return true;
        }

        private static Task ValidarCampos(IArticuloDTO articuloDTO)
        {
            if (string.IsNullOrWhiteSpace(articuloDTO.Numero))
                throw new ArgumentException("El número del artículo es obligatorio.");
            if (string.IsNullOrWhiteSpace(articuloDTO.Descripcion))
                throw new ArgumentException("La descripción del artículo es obligatoria.");
            return Task.CompletedTask;
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
    }
}
