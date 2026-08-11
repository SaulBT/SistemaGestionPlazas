using Microsoft.IdentityModel.Tokens;
using SGPla.Models.DTOs.Aviso;
using SGPla.Models.DTOs.Horario;
using SGPla.Validations.Interfaces;

namespace SGPla.Validations.Implementations
{
    public class AvisoValidator : IAvisoValidator
    {
        public Task ValidarCrearAviso(CrearAvisoDTO dto)
        {
            throw new NotImplementedException();
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }else
            {
                validarDatosAviso(dto);
                validarHorarios(dto.Horarios);
            }
        }

        private void validarDatosAviso(CrearAvisoDTO dto)
        {
            if(dto.Folio.IsNullOrEmpty())
                throw new ArgumentException(nameof(dto.Folio));
            if (dto.FechaCreacion != null)
                throw new ArgumentException(nameof(dto.FechaCreacion));

            //TODO
            throw new NotImplementedException();
        }

        private void validarHorarios(List<CrearHorarioAvisoDTO> horarios)
        {
            //TODO
            throw new NotImplementedException();
        }
    }
}
