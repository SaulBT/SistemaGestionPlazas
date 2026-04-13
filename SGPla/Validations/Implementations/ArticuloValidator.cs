using SGPla.Models.DTOs.Articulo;
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

        public async Task<bool> ValidarCreacionAsync(FormularioArticuloDTO crearArticuloDTO)
        {
            ArgumentNullException.ThrowIfNull(crearArticuloDTO);

            await ValidarCamposCreacion(crearArticuloDTO);
            await ValidarNoRepetidoAsync(crearArticuloDTO.Numero);
            return true;
        }

        private static Task ValidarCamposCreacion(FormularioArticuloDTO crearArticuloDTO)
        {
            if (string.IsNullOrWhiteSpace(crearArticuloDTO.Numero))
                throw new ArgumentException("El número del artículo es obligatorio.");
            if (string.IsNullOrWhiteSpace(crearArticuloDTO.Descripcion))
                throw new ArgumentException("La descripción del artículo es obligatoria.");
            return Task.CompletedTask;
        }

        private async Task ValidarNoRepetidoAsync(string numero)
        {
            bool existeNumero = await  _articuloRepository.ExisteNumeroAsync(numero);

            if (existeNumero)
            {
                throw new ArgumentException($"El número '{numero}' ya existe. Por favor, elija un número diferente.");
            }
                
        }
    }
}
