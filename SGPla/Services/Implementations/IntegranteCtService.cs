using SGPla.Models;
using SGPla.Models.DTOs.IntegranteCt;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Services.Implementations
{
    public class IntegranteCtService : IIntegranteCtService
    {
        private readonly IIntegranteCtRepository _repositorio;
        private readonly IIntegranteCtValidator _validator;

        public IntegranteCtService(IIntegranteCtRepository repositorio, IIntegranteCtValidator validator)
        {
            _repositorio = repositorio;
            _validator = validator;
        }

        public async Task RegistrarIntegranteAsync(RegistrarIntegranteCtDto integranteDto)
        {
            await _validator.ValidarRegistroAsync(integranteDto);

            var nombre = $"{integranteDto.Grado} {integranteDto.Nombre}";
            var integrante = new IntegranteCt
            {
                Nombre = nombre,
                Cargo = integranteDto.Cargo,
                IdEntidadAcademica = integranteDto.IdEntidadAcademica
            };
            await _repositorio.RegistrarAsync(integrante);
        }

        public async Task EditarIntegranteAsync(DatosIntegranteCtDto integranteDto)
        {
            await _validator.ValidarEdicionAsync(integranteDto);

            var nombre = $"{integranteDto.Grado} {integranteDto.Nombre}";
            var integrante = new IntegranteCt
            {
                Nombre = nombre,
                Cargo = integranteDto.Cargo,
                IdIntegranteCt = integranteDto.IdIntegranteCt,
                IdEntidadAcademica = integranteDto.IdEntidadAcademica
            };
            await _repositorio.EditarAsync(integrante);
        }

        public async Task<DatosIntegranteCtDto> ObtenerIntegrantePorIdAsync(int idIntegranteCt)
        {
            await _validator.ValidarIdIntegranteAsync(idIntegranteCt);

            var integrante = await _repositorio.ObtenerPorIdAsync(idIntegranteCt);
            return mapearDatos(integrante);
        }

        public async Task<List<DatosIntegranteCtDto>> ObtenerTodosIntegrantesAsync(int idEntidadAcademica)
        {
            await _validator.ValidarIdEntidadAsync(idEntidadAcademica);

            var integrantes = await _repositorio.ObtenerTodosAsync(idEntidadAcademica);
            return integrantes.Select(mapearDatos).ToList();
        }

        public async Task<(List<DatosIntegranteCtDto> items, int cantidad)> ObtenerTodosIntegrantesPorPaginaAsync(BusquedaIntegranteCtDto busquedaDto)
        {
            await _validator.ValidarIdEntidadAsync(busquedaDto.IdEntidadAcademica);

            List<IntegranteCt> integrantes = [];
            var total = 0;
            var idEntidadAcademica = busquedaDto.IdEntidadAcademica;
            var pagina = busquedaDto.Pagina;
            var cantidad = busquedaDto.Cantidad;

            if (!string.IsNullOrEmpty(busquedaDto.Nombre))
            {
                integrantes = await _repositorio.ObtenerPorNombrePaginaAsync(idEntidadAcademica, busquedaDto.Nombre, pagina, cantidad);
                total = await _repositorio.ContarAsync(idEntidadAcademica, busquedaDto.Nombre);
            }
            else
            {
                integrantes = await _repositorio.ObtenerPorPaginaAsync(idEntidadAcademica, pagina, cantidad);
                total = await _repositorio.ContarAsync(idEntidadAcademica);
            }

            return (integrantes.Select(mapearDatos).ToList(), total);
        }

        public async Task EliminarIntegranteAsync(int idIntegranteCt)
        {
            await _validator.ValidarIdIntegranteAsync(idIntegranteCt);

            var integrante = await _repositorio.ObtenerPorIdAsync(idIntegranteCt);
            await _repositorio.EliminarAsync(integrante);
        }

        private DatosIntegranteCtDto mapearDatos(IntegranteCt integrante)
        {
            var nombreGrado = integrante.Nombre;
            var grado = nombreGrado.Substring(0, nombreGrado.IndexOf('.')+1);
            var nombre = nombreGrado.Substring(nombreGrado.IndexOf('.') + 2);
            return new DatosIntegranteCtDto
            {
                IdIntegranteCt = integrante.IdIntegranteCt,
                Cargo = integrante.Cargo,
                Nombre = nombre,
                Grado = grado,
                IdEntidadAcademica = integrante.IdEntidadAcademica
            };
        }
    }
}
