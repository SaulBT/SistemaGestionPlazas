using SGPla.Models;
using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Services.Implementations
{
    public class EntidadAcademicaService : IEntidadAcademicaService
    {
        private readonly IEntidadAcademicaRepository _entidadAcademicaRepository;
        private readonly IEntidadAcademicaValidator _entidadAcademicaValidator;

        public EntidadAcademicaService(
            IEntidadAcademicaRepository entidadAcademicaRepository,
            IEntidadAcademicaValidator entidadAcademicaValidator)
        {
            _entidadAcademicaRepository = entidadAcademicaRepository;
            _entidadAcademicaValidator = entidadAcademicaValidator;
        }

        public async Task<int> CrearAsync(CrearEntidadAcademicaDTO dto)
        {
            await _entidadAcademicaValidator.ValidarCreacionAsync(dto);

            var entidad = new EntidadAcademica
            {
                IdAreaAcademica = dto.IdAreaAcademica,
                Nombre = dto.Nombre,
                CalleNumero = dto.CalleNumero,
                Colonia = dto.Colonia,
                Cp = dto.Cp,
                Municipio = dto.Municipio,
                Telefono = dto.Telefono,
                Conmutador = dto.Conmutador,
                Extension = dto.Extension,
                Fax = dto.Fax,
                Region = dto.Region
            };

            var entidadCreada = await _entidadAcademicaRepository.CrearAsync(entidad);
            return entidadCreada.IdEntidadAcademica;
        }

        public async Task EditarAsync(DatosEntidadAcademicaDTO dto)
        {
            await _entidadAcademicaValidator.ValidarEdicionAsync(dto);

            var entidad = new EntidadAcademica
            {
                IdEntidadAcademica = dto.IdEntidadAcademica,
                IdAreaAcademica = dto.IdAreaAcademica,
                Nombre = dto.Nombre,
                CalleNumero = dto.CalleNumero,
                Colonia = dto.Colonia,
                Cp = dto.Cp,
                Municipio = dto.Municipio,
                Telefono = dto.Telefono,
                Conmutador = dto.Conmutador,
                Extension = dto.Extension,
                Fax = dto.Fax,
                Region = dto.Region
            };

            await _entidadAcademicaRepository.ActualizarAsync(entidad);
        }

        public async Task EliminarAsync(int id)
        {
            await _entidadAcademicaValidator.ValidarIdAsync(id);

            var entidad = await _entidadAcademicaRepository.ObtenerPorIdAsync(id);
            await _entidadAcademicaRepository.EliminarAsync(entidad);
        }

        public async Task<List<ListaEntidadAcademicaDTO>> ObtenerListaAsync(int indice)
        {
            _entidadAcademicaValidator.ValidarIndice(indice);

            var entidades = await _entidadAcademicaRepository.ObtenerDiezAsync(indice);
            var dtos = entidades.Select(mapearLista);

            return dtos.ToList();
        }

        public async Task<List<ListaEntidadAcademicaDTO>> ObtenerPorFiltroAsync(FiltroEntidadAcademicaDTO filtro, int indice)
        {
            _entidadAcademicaValidator.ValidarIndice(indice);

            var entidades = await _entidadAcademicaRepository.ObtenerPorFiltroAsync(filtro.Region, filtro.IdAreaAcademica, filtro.Nombre);
            var dtos = entidades.Select(mapearLista);

            return dtos.ToList();
        }

        public async Task<DatosEntidadAcademicaDTO> ObtenerPorIdAsync(int id)
        {
            await _entidadAcademicaValidator.ValidarIdAsync(id);

            var entidad = await _entidadAcademicaRepository.ObtenerPorIdAsync(id);
            return mapearDto(entidad);

        }

        private DatosEntidadAcademicaDTO mapearDto(EntidadAcademica entidad)
        {
            var clave = entidad.Nombre[..5];
            var nombre = entidad.Nombre[6..];

            return new DatosEntidadAcademicaDTO
            {
                IdEntidadAcademica = entidad.IdEntidadAcademica,
                IdAreaAcademica = entidad.IdAreaAcademica,
                Clave = clave,
                Nombre = nombre,
                CalleNumero = entidad.CalleNumero,
                Colonia = entidad.Colonia,
                Cp = entidad.Cp,
                Municipio = entidad.Municipio,
                Telefono = entidad.Telefono,
                Conmutador = entidad.Conmutador,
                Extension = entidad.Extension,
                Fax = entidad.Fax,
                NombreEntidadAcademica = entidad.IdAreaAcademicaNavigation.Nombre,
                Region = entidad.Region
            };
        }

        private ListaEntidadAcademicaDTO mapearLista(EntidadAcademica entidad)
        {
            string domicilio = "";
            string telefono = "";

            return new ListaEntidadAcademicaDTO
            {
                IdEntidadAcademica = entidad.IdEntidadAcademica,
                IdAreaAcademica = entidad.IdAreaAcademica,
                Nombre = entidad.Nombre,
                Domicilio = domicilio,
                Telefono = telefono,
                NombreEntidadAcademica = entidad.IdAreaAcademicaNavigation.Nombre,
                Region = entidad.Region
            };
        }
    }
}
