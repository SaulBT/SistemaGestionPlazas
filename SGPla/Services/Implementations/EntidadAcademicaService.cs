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

        public async Task<(List<ListaEntidadAcademicaDTO> Items, int TotalCount)> BuscarPorFiltroPaginadoAsync(FiltroEntidadAcademicaDTO filtro)
        {
            _entidadAcademicaValidator.ValidarIndice(filtro.Pagina);

            var entidades = await _entidadAcademicaRepository.ObtenerPorFiltroAsync(filtro.Region, filtro.IdAreaAcademica, filtro.Nombre, filtro.Pagina, filtro.Cantidad);
            var cantidad = await _entidadAcademicaRepository.ContarPorFiltroAsync(filtro.Region, filtro.IdAreaAcademica, filtro.Nombre);
            var dtos = entidades.Select(mapearLista);

            return (dtos.ToList(), cantidad);
        }

        public async Task<int> CrearAsync(CrearEntidadAcademicaDTO dto)
        {
            await _entidadAcademicaValidator.ValidarCreacionAsync(dto);

            var entidad = new EntidadAcademica
            {
                IdAreaAcademica = dto.IdAreaAcademica.Value,
                IdRegion = ObtenerIdRegion(dto.Region),
                Clave = dto.Clave.Trim(),
                Nombre = dto.Nombre.Trim(),
                CalleNumero = dto.CalleNumero,
                Colonia = dto.Colonia,
                Cp = dto.Cp,
                Municipio = dto.Municipio,
                Telefono = dto.Telefono,
                Extension = dto.Extension,
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
                IdRegion = ObtenerIdRegion(dto.Region),
                Clave = dto.Clave.Trim(),
                Nombre = dto.Nombre.Trim(),
                CalleNumero = dto.CalleNumero,
                Colonia = dto.Colonia,
                Cp = dto.Cp,
                Municipio = dto.Municipio,
                Telefono = dto.Telefono,
                Extension = dto.Extension,
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

        public async Task<List<ListaEntidadAcademicaDTO>> ObtenerCatalogoAsync(FiltroEntidadAcademicaDTO filtro)
        {
            var entidades = await _entidadAcademicaRepository.ObtenerOpcionesAsync(filtro.Region, filtro.IdAreaAcademica);
            var dtos = entidades.Select(e => new ListaEntidadAcademicaDTO
            {
                IdEntidadAcademica = e.IdEntidadAcademica,
                Nombre = e.Nombre
            });

            return dtos.ToList();
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

            var entidades = await _entidadAcademicaRepository.ObtenerPorFiltroAsync(filtro.Region, filtro.IdAreaAcademica, filtro.Nombre, indice, filtro.Cantidad);
            var dtos = entidades.Select(mapearLista);

            return dtos.ToList();
        }

        public async Task<DatosEntidadAcademicaDTO> ObtenerPorIdAsync(int id)
        {
            await _entidadAcademicaValidator.ValidarIdAsync(id);

            var entidad = await _entidadAcademicaRepository.ObtenerPorIdAsync(id);
            return mapearDto(entidad);

        }

        private static int? ObtenerIdRegion(string? region)
        {
            if (string.IsNullOrWhiteSpace(region))
                return null;

            var valor = region.Trim();
            var separador = valor.IndexOf('-');
            var codigo = separador > 0 ? valor[..separador] : valor;

            if (int.TryParse(codigo, out var id) && id > 0)
                return id;

            var indice = SGPla.Commons.Constantes.REGIONES
                .Select((nombre, posicion) => new { nombre, posicion })
                .FirstOrDefault(item => item.nombre.EndsWith(valor, StringComparison.OrdinalIgnoreCase));

            return indice is null ? null : indice.posicion + 1;
        }

        private DatosEntidadAcademicaDTO mapearDto(EntidadAcademica entidad)
        {
            var (clave, nombre) = SepararClaveYNombre(entidad.Clave, entidad.Nombre);

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
                Extension = entidad.Extension,
                NombreEntidadAcademica = entidad.IdAreaAcademicaNavigation.Nombre,
                Region = entidad.Region
            };
        }

        private static (string Clave, string Nombre) SepararClaveYNombre(string? clave, string nombre)
        {
            var nombreLimpio = nombre.Trim();
            var claveLimpia = clave?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(claveLimpia))
            {
                if (nombreLimpio.StartsWith(claveLimpia + "-", StringComparison.Ordinal))
                    return (claveLimpia, nombreLimpio[(claveLimpia.Length + 1)..].Trim());

                return (claveLimpia, nombreLimpio);
            }

            var separador = nombreLimpio.IndexOf('-');
            if (separador > 0 && separador + 1 < nombreLimpio.Length)
            {
                var posibleClave = nombreLimpio[..separador].Trim();
                if (posibleClave.Length == 5 && posibleClave.All(char.IsDigit))
                    return (posibleClave, nombreLimpio[(separador + 1)..].Trim());
            }

            return (string.Empty, nombreLimpio);
        }

        private ListaEntidadAcademicaDTO mapearLista(EntidadAcademica entidad)
        {
            var (_, nombre) = SepararClaveYNombre(entidad.Clave, entidad.Nombre);
            string domicilio = $"{entidad.CalleNumero} Col. {entidad.Colonia} C.P. {entidad.Cp} {entidad.Municipio}";
            string telefono = $"Teléfono: {entidad.Telefono} Ext: {entidad.Extension}";

            return new ListaEntidadAcademicaDTO
            {
                IdEntidadAcademica = entidad.IdEntidadAcademica,
                IdAreaAcademica = entidad.IdAreaAcademica,
                Nombre = nombre,
                Domicilio = domicilio,
                Telefono = telefono,
                NombreAreaAcademica = entidad.IdAreaAcademicaNavigation.Nombre,
                Region = entidad.Region
            };
        }
    }
}
