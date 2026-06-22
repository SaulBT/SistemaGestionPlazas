using SGPla.Commons;
using SGPla.Models.DTOs.IntegranteCt;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Validations.Implementations
{
    public class IntegranteCtValidator : IIntegranteCtValidator
    {
        private readonly IIntegranteCtRepository _integranteRepositorio;
        private readonly IEntidadAcademicaRepository _entidadRepositorio;

        public IntegranteCtValidator(IIntegranteCtRepository integranteRepositorio, IEntidadAcademicaRepository entidadRepositorio)
        {
            _integranteRepositorio = integranteRepositorio;
            _entidadRepositorio = entidadRepositorio;
        }

        public async Task ValidarRegistroAsync(RegistrarIntegranteCtDto integranteDto)
        {
            if (integranteDto == null)
                throw new ValidacionExcepction("No se enviaron datos", "400");

            await validarEntidadAsync(integranteDto.IdEntidadAcademica);
            await validarCamposAsync(integranteDto.Nombre, integranteDto.Cargo, integranteDto.Grado, integranteDto.IdEntidadAcademica, -1);
        }

        public async Task ValidarEdicionAsync(DatosIntegranteCtDto integranteDto)
        {
            if (integranteDto == null)
                throw new ValidacionExcepction("No se enviaron datos", "400");

            await validarIntegranteAsync(integranteDto.IdIntegranteCt);
            await validarEntidadAsync(integranteDto.IdEntidadAcademica);
            await validarCamposAsync(integranteDto.Nombre, integranteDto.Cargo, integranteDto.Grado, integranteDto.IdEntidadAcademica, integranteDto.IdIntegranteCt);
        }

        public async Task ValidarIdIntegranteAsync(int idIntegrante)
        {
            await validarIntegranteAsync(idIntegrante);
        }

        public async Task ValidarIdEntidadAsync(int idEntidad)
        {
            await validarEntidadAsync(idEntidad);
        }

        private async Task validarCamposAsync(string nombre, string cargo, string grado, int idEntidadAcademica, int idIntegrante)
        {
            if (string.IsNullOrEmpty(cargo))
                throw new ValidacionExcepction("El Cargo es obligatorio.", "400");
            if (string.IsNullOrEmpty(nombre))
                throw new ValidacionExcepction("El Nombre es obligatorio.", "400");
            if (string.IsNullOrEmpty(grado))
                throw new ValidacionExcepction("El Grado es obligatorio.", "400");
            if (!Constantes.GRADOS_INTEGRANTE.Contains(grado))
                throw new ValidacionExcepction("El Grado es inválido.", "400");
            if (await _integranteRepositorio.ExistePorNombre(nombre, idEntidadAcademica, idIntegrante))
                throw new ValidacionExcepction("Ya existe ese Integrante", "409");
        }

        private async Task validarIntegranteAsync(int idIntegranteCt)
        {
            if (idIntegranteCt < 1)
                throw new ValidacionExcepction("La IdIntegranteCt es inválida.", "400");
            if (!await _integranteRepositorio.ExistePorIdAsync(idIntegranteCt))
                throw new ValidacionExcepction("No existe ese Integrante.", "404");
        }

        private async Task validarEntidadAsync(int idEntidadAcademica)
        {
            if (idEntidadAcademica < 1)
                throw new ValidacionExcepction("La IdEntidadAcademica es inválida.", "400");
            if (!await _entidadRepositorio.ExistePorIdAsync(idEntidadAcademica))
                throw new ValidacionExcepction("No existe esa Entidad Academica.", "404");
        }
    }
}
