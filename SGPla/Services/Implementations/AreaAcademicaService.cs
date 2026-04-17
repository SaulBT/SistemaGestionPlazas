using SGPla.Models;
using SGPla.Models.DTOs.AreaAcademica;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Services.Implementations
{
    public class AreaAcademicaService : IAreaAcademicaService
    {
        private readonly IAreaAcademicaRepository _areaAcademicaRepository;
        private readonly IAreaAcademicaValidator _areaAcademicaValidator;

        public AreaAcademicaService(
                IAreaAcademicaRepository areaAcademicaRepository,
                IAreaAcademicaValidator areaAcademicaValidator
            )
        {
            _areaAcademicaRepository = areaAcademicaRepository;
            _areaAcademicaValidator = areaAcademicaValidator;
        }

        public async Task<int> CrearAsync(CrearAreaAcademicaDTO dto)
        {
            _areaAcademicaValidator.ValidarCreacion(dto);

            var areaAcademica = new AreaAcademica
            {
                Nombre = dto.Nombre,
                CalleNumero = dto.CalleNumero,
                Colonia = dto.Colonia,
                Cp = dto.Cp,
                Municipio = dto.Municipio,
                Telefono = dto.Telefono,
                Conmutador = dto.Conmutador,
                Extension = dto.Extension,
                Fax = dto.Fax
            };

            var areaAcademicaCreada = await _areaAcademicaRepository.CrearAsync(areaAcademica);
            return areaAcademicaCreada.IdAreaAcademica;
        }

        public async Task<List<ListaAreaAcademicaDTO>> ObtenerTodasAsync()
        {
            var listaAreasAcademicas = await _areaAcademicaRepository.ObtenerTodosAsync();
            var listaDtos = listaAreasAcademicas.Select(generarListaAreaAcademicaDTO);

            return listaDtos.ToList();
        }

        public async Task<List<ListaAreaAcademicaDTO>> ObtenerPorNombreAsync(string nombre)
        {
            var listaAreasAcademicas = await _areaAcademicaRepository.ObtenerPorNombreAsync(nombre);
            var listaDtos = listaAreasAcademicas.Select(generarListaAreaAcademicaDTO);

            return listaDtos.ToList();
        }

        public async Task<DatosAreaAcademicaDTO> ObtenerPorIdAsync(int id)
        {
            await _areaAcademicaValidator.ValidarIdAsync(id);

            var areaAcademica = await _areaAcademicaRepository.ObtenerPorIdAsync(id);

            return new DatosAreaAcademicaDTO
            {
                IdAreaAcademica = areaAcademica.IdAreaAcademica,
                Nombre = areaAcademica.Nombre,
                CalleNumero = areaAcademica.CalleNumero,
                Colonia = areaAcademica.Colonia,
                Cp = areaAcademica.Cp,
                Municipio = areaAcademica.Municipio,
                Telefono = areaAcademica.Telefono,
                Conmutador = areaAcademica.Conmutador,
                Extension = areaAcademica.Extension,
                Fax = areaAcademica.Fax
            };
        }

        public async Task EditarAsync(DatosAreaAcademicaDTO dto)
        {
            await _areaAcademicaValidator.ValidarEdicionAsync(dto);

            var areaAcademica = new AreaAcademica
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
                Fax = dto.Fax
            };

            await _areaAcademicaRepository.ActualizarAsync(areaAcademica);
        }

        public async Task EliminarAsync(int id)
        {
            await _areaAcademicaValidator.ValidarIdAsync(id);
            var areaAcademica = await _areaAcademicaRepository.ObtenerPorIdAsync(id);
            await _areaAcademicaRepository.EliminarAsync(areaAcademica);
        }

        private ListaAreaAcademicaDTO generarListaAreaAcademicaDTO(AreaAcademica areaAcademica)
        {
            var domicilio = string.Concat(
                areaAcademica.CalleNumero,
                " Col. ",
                areaAcademica.Colonia,
                " C.P. ",
                areaAcademica.Cp,
                " ",
                areaAcademica.Municipio
            );

            var telefono = string.Concat(
                    "Teléfono: ",
                    areaAcademica.Telefono,
                    "\nConmutador: ",
                    areaAcademica.Conmutador,
                    " Ext: ",
                    areaAcademica.Extension,
                    "\nFax: ",
                    areaAcademica.Fax
                );

            return new ListaAreaAcademicaDTO
            {
                IdAreaAcademica = areaAcademica.IdAreaAcademica,
                Nombre = areaAcademica.Nombre,
                Domicilio = domicilio,
                Telefono = telefono
            };
        }
    }
}
