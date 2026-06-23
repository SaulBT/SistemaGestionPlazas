using SGPla.Models;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Docentes;
using SGPla.Models.DTOs.Grados;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Services.Implementations
{
    public class DocenteService : IDocenteService
    {
        private readonly IDocenteRepository _docenteRepository;
        private readonly IGradoRepository _gradoRepository;
        private readonly IArchivoRepository _archivoRepository;
        private readonly IArchivoService _archivoService;
        private readonly IDocenteValidator _validacion;

        public DocenteService(
            IDocenteRepository docenteRepository,
            IGradoRepository gradoRepository,
            IArchivoRepository archivoRepository,
            IArchivoService archivoService,
            IDocenteValidator validacion)
        {
            _docenteRepository = docenteRepository;
            _gradoRepository = gradoRepository;
            _archivoRepository = archivoRepository;
            _archivoService = archivoService;
            _validacion = validacion;
        }

        public async Task RegistrarDocenteAsync(RegistrarDocenteDTO docente)
        {
            await _validacion.ValidarRegistroAsync(docente);
            DatosArchivoGuardadoDTO? archivoGuardado = null;
            Archivo? archivoRegistrado = null;

            try
            {
                archivoGuardado = await _archivoService.GuardarAsync(docente.ArchivosGenerales.RutaArchivo, docente.ArchivosGenerales.NombreArchivo, "archivos-docente");
                archivoRegistrado = await _archivoRepository.CrearAsync(new Archivo
                {
                    Nombre = archivoGuardado.NombreOriginal,
                    Ruta = archivoGuardado.Ruta,
                    Tipo = archivoGuardado.Tipo,
                    Tamanio = archivoGuardado.Tamanio
                });

                var docenteRegistrado = new Docente
                {
                    Nombre = docente.Nombre,
                    DescripcionPerfil = docente.DescripcionPerfil,
                    IdArchivosGenerales = archivoRegistrado.IdArchivo,
                    NumeroPersonal = docente.NumeroPersonal,
                    Puesto = docente.Puesto
                };
                docenteRegistrado = await _docenteRepository.RegistrarAsync(docenteRegistrado);

                foreach (var grado in docente.Grados) {
                    await _gradoRepository.AgregarAsync(new Grado
                    {
                        IdDocente = docenteRegistrado.IdDocente,
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

        public async Task<DatosDocenteDTO> ObtenerDocenteAsync(int idDocente)
        {
            await _validacion.ValidarIdAsync(idDocente);
            var docente = await _docenteRepository.ObtenerPorIdAsync(idDocente);
            var grados = await _gradoRepository.ObtenerTodosAsync(idDocente);

            return new DatosDocenteDTO
            {
                IdDocente = docente.IdDocente,
                Nombre = docente.Nombre,
                DescripcionPerfil = docente.DescripcionPerfil,
                IdArchivosGenerales = (int)docente.IdArchivosGenerales,
                Grados = grados.Select(mapearGradoAGradoDTO).ToList(),
                NumeroPersonal = docente.NumeroPersonal,
                Puesto = docente.Puesto
            };
        }

        public async Task<(List<ListaDocenteDTO> items, int total)> ObtenerTodosDocentesAsync(string busqueda, int pagina, int cantidad)
        {
            var docentes = await _docenteRepository.ObtenerPorPaginaAsync(busqueda, pagina, cantidad);
            var total = await _docenteRepository.ContarAsync(busqueda);
            var items = new List<ListaDocenteDTO>();
            foreach (var docente in docentes)
            {
                items.Add(new ListaDocenteDTO
                {
                    IdDocente = docente.IdDocente,
                    Nombre = docente.Nombre,
                    NumeroPersonal = docente.NumeroPersonal,
                    Puesto = docente.Puesto,
                    IdArchivosGenerales = (int)docente.IdArchivosGenerales
                });
            }

            return (items, total);
        }

        public async Task EditarDocenteAsync(EditarDocenteDTO dto)
        {
            await _validacion.ValidarEdicionAsync(dto);

            var docente = await _docenteRepository.ObtenerPorIdAsync(dto.IdDocente);
            DatosArchivoGuardadoDTO? archivoNuevoGuardado = null;
            Archivo? archivoAnterior = null;
            string? rutaAnterior = null;

            try
            {
                if (dto.NuevoArchivo && dto.ArchivosGenerales != null)
                {
                    archivoNuevoGuardado = await _archivoService.GuardarAsync(dto.ArchivosGenerales.RutaArchivo, dto.ArchivosGenerales.NombreArchivo, "archivos-docente");
                    archivoAnterior = await _archivoRepository.ObtenerPorIdAsync((int)docente.IdArchivosGenerales);

                    if (archivoAnterior != null)
                    {
                        rutaAnterior = archivoAnterior.Ruta;

                        archivoAnterior.Nombre = archivoNuevoGuardado.NombreOriginal;
                        archivoAnterior.Ruta = archivoNuevoGuardado.Ruta;
                        archivoAnterior.Tipo = archivoNuevoGuardado.Tipo;
                        archivoAnterior.Tamanio = archivoNuevoGuardado.Tamanio;
                        //await _archivoRepository.EliminarAsync(archivoAnterior);
                    }
                }

                docente.Nombre = dto.Nombre;
                docente.DescripcionPerfil = dto.DescripcionPerfil;
                docente.NumeroPersonal = dto.NumeroPersonal;
                docente.Puesto = dto.Puesto;

                if (dto.GradosAgregados.Count > 0)
                {
                    var gradosAgregados = dto.GradosAgregados.Select(g => mapearAgregadoGradoAGrado(g, docente.IdDocente)).ToList();
                    foreach (var grado in gradosAgregados)
                    {
                        await _gradoRepository.AgregarAsync(grado);
                    }
                }

                if (dto.GradosEditados.Count > 0)
                {
                    var datosGrados = dto.GradosEditados.Select(g => mapearDatosGradoAGrado(g, docente.IdDocente)).ToList();
                    foreach (var grado in datosGrados)
                    {
                        await _gradoRepository.EditarAsync(grado);
                    }
                }

                if (dto.IdsGradosEliminados.Count > 0)
                {
                    foreach (var idGrado in dto.IdsGradosEliminados)
                    {
                        var grado = await _gradoRepository.ObtenerAsync(idGrado);
                        if (grado != null)
                            await _gradoRepository.EliminarAsync(grado);
                    }
                }

                await _docenteRepository.EditarAsync(docente);

                if (!string.IsNullOrWhiteSpace(rutaAnterior) && archivoNuevoGuardado != null)
                    await _archivoService.EliminarAsync(rutaAnterior);
            }
            catch
            {
                if (archivoNuevoGuardado != null)
                    await _archivoService.EliminarAsync(archivoNuevoGuardado.Ruta);

                throw;
            }

        }

        public async Task EliminarDocenteAsync(int idDocente)
        {
            await _validacion.ValidarIdAsync(idDocente);

            var docente = await _docenteRepository.ObtenerPorIdAsync(idDocente);
            var grados = await _gradoRepository.ObtenerTodosAsync(idDocente);
            var archivo = await _archivoRepository.ObtenerPorIdAsync((int)docente.IdArchivosGenerales);

            foreach (var grado in grados)
            {
                await _gradoRepository.EliminarAsync(grado);
            }
            if (archivo != null)
            {
                await _archivoRepository.EliminarAsync(archivo);
                await _archivoService.EliminarAsync(archivo.Ruta);
            }
            await _docenteRepository.EliminarAsync(docente);
        }

        private DatosGradoDTO mapearGradoAGradoDTO(Grado grado)
        {
            return new DatosGradoDTO
            {
                IdGrado = grado.IdGrado,
                IdDocente = grado.IdDocente,
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
