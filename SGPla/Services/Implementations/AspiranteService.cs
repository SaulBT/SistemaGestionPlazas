using SGPla.Models;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Docentes;
using SGPla.Models.DTOs.Grados;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Services.Implementations
{
    public class AspiranteService : IAspiranteService
    {
        private readonly IAspiranteRepository _aspiranteRepository;
        private readonly IGradoRepository _gradoRepository;
        private readonly IArchivoRepository _archivoRepository;
        private readonly IArchivoService _archivoService;
        private readonly IAspiranteValidator _validacion;

        public AspiranteService(
            IAspiranteRepository aspiranteRepository,
            IGradoRepository gradoRepository,
            IArchivoRepository archivoRepository,
            IArchivoService archivoService,
            IAspiranteValidator validacion)
        {
            _aspiranteRepository = aspiranteRepository;
            _gradoRepository = gradoRepository;
            _archivoRepository = archivoRepository;
            _archivoService = archivoService;
            _validacion = validacion;
        }

        public async Task RegistrarAspiranteAsync(RegistrarDocenteDTO dto)
        {
            await _validacion.ValidarRegistroAsync(dto);

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
            await _validacion.ValidarIdAsync(idDocente);

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
                var ultimoGrado = "";
                foreach (var grado in grados)
                {
                    if (grado.Ultimo)
                        ultimoGrado = grado.Grado1;
                }

                items.Add(new ListaAspiranteDTO
                {
                    IdDocente = aspirante.IdDocente,
                    Nombre = aspirante.Nombre,
                    DescripcionPerfil = aspirante.DescripcionPerfil,
                    IdArchivosGenerales = (int)aspirante.IdArchivosGenerales,
                    UltimoGrado = ultimoGrado
                });
            }

            return (items, total);
        }

        public async Task EditarAspiranteAsync(EditarDocenteDTO dto)
        {
            await _validacion.ValidarEdicionAsync(dto);

            var aspirante = await _aspiranteRepository.ObtenerPorIdAsync(dto.IdDocente);
            DatosArchivoGuardadoDTO? archivoNuevoGuardado = null;
            Archivo? archivoAnterior = null;
            string? rutaAnterior = null;

            try
            {
                if (dto.NuevoArchivo && dto.ArchivosGenerales != null)
                {
                    archivoNuevoGuardado = await _archivoService.GuardarAsync(dto.ArchivosGenerales.RutaArchivo, dto.ArchivosGenerales.NombreArchivo, "archivos-docente");
                    archivoAnterior = await _archivoRepository.ObtenerPorIdAsync((int)aspirante.IdArchivosGenerales);

                    if (archivoAnterior != null)
                    {
                        rutaAnterior = archivoAnterior.Ruta;

                        archivoAnterior.Nombre = archivoNuevoGuardado.NombreOriginal;
                        archivoAnterior.Ruta = archivoNuevoGuardado.Ruta;
                        archivoAnterior.Tipo = archivoNuevoGuardado.Tipo;
                        archivoAnterior.Tamanio = archivoNuevoGuardado.Tamanio;
                    }
                }

                aspirante.Nombre = dto.Nombre;
                aspirante.DescripcionPerfil = dto.DescripcionPerfil;

                if (dto.GradosAgregados.Count > 0)
                {
                    var gradosAgregados = dto.GradosAgregados.Select(g => mapearAgregadoGradoAGrado(g, aspirante.IdDocente)).ToList();
                    foreach (var grado in gradosAgregados)
                    {
                        await _gradoRepository.AgregarAsync(grado);
                    }
                }

                if (dto.GradosEditados.Count > 0)
                {
                    var datosGrados = dto.GradosEditados.Select(g => mapearDatosGradoAGrado(g, aspirante.IdDocente)).ToList();
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

                await _aspiranteRepository.EditarAsync(aspirante);

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

        public async Task EliminarAspiranteAsync(int idDocente)
        {
            await _validacion.ValidarIdAsync(idDocente);
            var docente = await _aspiranteRepository.ObtenerPorIdAsync(idDocente);
            var grados = await _gradoRepository.ObtenerTodosAsync(idDocente);
            var archivo = await _archivoRepository.ObtenerPorIdAsync((int)docente.IdArchivosGenerales);

            foreach (var grado in grados)
            {
                await _gradoRepository.EliminarAsync(grado);
            }
            
            await _aspiranteRepository.EliminarAsync(docente);

            if (archivo != null)
            {
                await _archivoRepository.EliminarAsync(archivo);
                await _archivoService.EliminarAsync(archivo.Ruta);
            }
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
