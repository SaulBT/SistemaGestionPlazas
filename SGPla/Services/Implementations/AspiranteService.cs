using SGPla.Models;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Docentes;
using SGPla.Models.DTOs.Grados;
using SGPla.Repositories.Implementations;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations
{
    public class AspiranteService : IAspiranteService
    {
        private readonly IAspiranteRepository _aspiranteRepository;
        private readonly IGradoRepository _gradoRepository;
        private readonly IArchivoRepository _archivoRepository;
        private readonly IArchivoService _archivoService;

        public AspiranteService(
            IAspiranteRepository aspiranteRepository,
            IGradoRepository gradoRepository,
            IArchivoRepository archivoRepository,
            IArchivoService archivoService)
        {
            _aspiranteRepository = aspiranteRepository;
            _gradoRepository = gradoRepository;
            _archivoRepository = archivoRepository;
            _archivoService = archivoService;
        }

        public async Task RegistrarAspiranteAsync(RegistrarDocenteDTO dto)
        {
            DatosArchivoGuardadoDTO? archivoGuardado = null;
            Archivo? archivoRegistrado = null;

            try
            {
                archivoGuardado = await _archivoService.GuardarAsync(dto.ArchivosGenerales.RutaArchivo, dto.ArchivosGenerales.NombreArchivo, "archivos-docente");
                archivoRegistrado = await _archivoRepository.CrearAsync(new Archivo
                {
                    Nombre = archivoGuardado.NombreOriginal,
                    Ruta = archivoGuardado.Ruta,
                    Tipo = archivoGuardado.Tipo,
                    Tamanio = archivoGuardado.Tamanio
                });

                var aspirtante = new Docente
                {
                    Nombre = dto.Nombre,
                    DescripcionPerfil = dto.DescripcionPerfil,
                    IdArchivosGenerales = archivoRegistrado.IdArchivo
                };
                aspirtante = await _aspiranteRepository.RegistrarAsync(aspirtante);

                foreach (var grado in dto.Grados)
                {
                    await _gradoRepository.AgregarAsync(new Grado
                    {
                        IdDocente = aspirtante.IdDocente,
                        Grado1 = grado.Grado,
                        Titulo = grado.Titulo,
                        Ultimo = grado.Ultimo
                    });
                }
            }
            catch
            {
                if (archivoRegistrado != null)
                    await _archivoRepository.EliminarAsync(archivoRegistrado);
                if (archivoGuardado != null)
                    await _archivoService.EliminarAsync(archivoGuardado.Ruta);
            }
        }

        public async Task<DatosDocenteDTO> ObtenerAspiranteAsync(int idDocente)
        {
            var aspirante = await _aspiranteRepository.ObtenerPorIdAsync(idDocente);
            var grados = await _gradoRepository.ObtenerTodosAsync(idDocente);

            return new DatosDocenteDTO
            {
                IdDocente = aspirante.IdDocente,
                Nombre = aspirante.Nombre,
                DescripcionPerfil = aspirante.DescripcionPerfil,
                IdArchivosGenerales = (int)aspirante.IdArchivosGenerales,
                Grados = grados.Select(mapearGradoAGradoDTO).ToList()
            };
        }

        public async Task<(List<ListaAspiranteDTO> items, int total)> ObtenerTodosAspirantesAsync(string busqueda, int pagina, int cantidad)
        {
            var aspirantes = await _aspiranteRepository.ObtenerPorPaginaAsync(busqueda, pagina, cantidad);
            var total = await _aspiranteRepository.ContarAsync(busqueda);
            var items = new List<ListaAspiranteDTO>();
            foreach (var aspirante in aspirantes)
            {
                var grados = await _gradoRepository.ObtenerTodosAsync(aspirante.IdDocente);
                //TODO: Agregar el ultimo grado al DTO y CAMBIAR LA BD PARA QUE IdArchivosGenerales NO SEA NULLABLE
                items.Add(new ListaAspiranteDTO
                {
                    IdDocente = aspirante.IdDocente,
                    Nombre = aspirante.Nombre,
                    IdArchivosGenerales = (int)aspirante.IdArchivosGenerales
                });
            }

            return (items, total);
        }

        private DatosGradoDTO mapearGradoAGradoDTO(Grado grado)
        {
            return new DatosGradoDTO
            {
                IdGrado = grado.IdGrado,
                Grado = grado.Grado1,
                Titulo = grado.Titulo,
                Ultimo = grado.Ultimo
            };
        }

        private Grado mapearAgregadoGradoAGrado(AgregarGradoDTO gradoAgregado, int idDocente)
        {
            return new Grado
            {
                IdDocente = idDocente,
                Grado1 = gradoAgregado.Grado,
                Titulo = gradoAgregado.Titulo,
                Ultimo = gradoAgregado.Ultimo
            };
        }

        private Grado mapearDatosGradoAGrado(DatosGradoDTO datosGrado, int idDocente)
        {
            return new Grado
            {
                IdGrado = datosGrado.IdGrado,
                IdDocente = idDocente,
                Grado1 = datosGrado.Grado,
                Titulo = datosGrado.Titulo,
                Ultimo = datosGrado.Ultimo
            };
        }
    }
}
