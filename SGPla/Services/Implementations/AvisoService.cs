using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SGPla.Mappers;
using SGPla.Models;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Aviso;
using SGPla.Models.DTOs.Horario;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.PeriodoEscolar;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Models.DTOs.Plantillas;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using SGPla.Validations.Interfaces;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SGPla.Services.Implementations
{
    public class AvisoService : IAvisoService
    {
        private readonly IAvisoRepository _avisoRepository;
        private readonly IProgramacionAcademicaRepository _programacionAcademicaRepository;
        private readonly IOfertaRepository _ofertaRepository;
        private readonly IArchivoRepository _archivoRepository;
        private readonly IArchivoService _archivoService;
        private readonly IHorarioRepository _horarioRepository;
        private readonly IAvisoValidator _avisoValidator;
        private readonly IPeriodoEscolarRepository _periodoEscolarRepository;
        private readonly IEntidadAcademicaRepository _entidadAcademicaRepository;
        private readonly IArticuloRepository _articuloRepository;
        private readonly IPlantillaService _plantillaService;

        public AvisoService(
            IAvisoRepository avisoRepository, 
            IProgramacionAcademicaRepository programacionAcademicaRepository, 
            IOfertaRepository ofertaRepository,
            IArchivoRepository archivoRepository,
            IArchivoService archivoService,
            IHorarioRepository horarioRepository,
            IAvisoValidator avisoValidator,
            IPeriodoEscolarRepository periodoEscolarRepository,
            IEntidadAcademicaRepository entidadAcademicaRepository,
            IArticuloRepository articuloRepository,
            IPlantillaService plantillaService)
        {
            _avisoRepository = avisoRepository;
            _programacionAcademicaRepository = programacionAcademicaRepository;
            _ofertaRepository = ofertaRepository;
            _archivoRepository = archivoRepository;
            _archivoService = archivoService;
            _horarioRepository = horarioRepository;
            _avisoValidator = avisoValidator;
            _periodoEscolarRepository = periodoEscolarRepository;
            _entidadAcademicaRepository = entidadAcademicaRepository;
            _archivoRepository = archivoRepository;
            _plantillaService = plantillaService;
            _articuloRepository = articuloRepository;
        }

        public async Task<(List<ListaAvisosDTO> items, int total)> ObtenerTodosAvisosAsync(FiltroAvisosDTO filtroDTO)
        {
            var avisos = await _avisoRepository.ObtenerTodosAsync(filtroDTO);

            var items = (await Task.WhenAll(avisos.Select(mapearAListaAvisoDTOAsync))).ToList();
            var total = await _avisoRepository.ContarAsync(filtroDTO);

            return (items, total);
        }

        public async Task<DatosAvisoDTO> ObtenerAvisoPorIDAsync(int idAviso)
        {
            await _avisoValidator.ValidarIdAsync(idAviso);

            var aviso = await _avisoRepository.ObtenerPorIDAsync(idAviso);
            var periodoDTO = await obtenerPeriodoDTOAsync(aviso.IdPeriodo);
            var ofertas = await obtenerOfertasDeAvisoAsync(idAviso);
            var horario = await obtenerHorarioAvisoAsync(idAviso);

            return new DatosAvisoDTO
            {
                IdAviso = aviso.IdAviso,
                IdEntidadAcademica = aviso.IdEntidadAcademica,
                IdPeriodo = aviso.IdPeriodo,
                IdArticulo = aviso.IdArticulo,
                IdArchivoOriginal = aviso.IdArchivoOriginal ?? 0,
                IdArchivoFirmado = aviso.IdArchivoFirmado ?? 0,
                Articulo = aviso.IdArticuloNavigation.Numero,
                FechaCreacion = aviso.FechaCreacion.ToString("dd/MM/yyyy"),
                FechaVacantes = aviso.FechaVacantes.ToString("dd/MM/yyyy"),
                Region = aviso.IdEntidadAcademicaNavigation.Region,
                NombreEntidadAcademica = aviso.IdEntidadAcademicaNavigation.Nombre,
                Periodo = periodoDTO.PeriodoMostrar,
                Folio = aviso.Folio,
                Sistema = "",
                Ofertas = ofertas,
                Requisitos = aviso.Requisitos,
                Modalidad = aviso.Modalidad,
                Lugar = aviso.Lugar ?? "",
                Horario = horario,
                UrlPublicacion = ""
            };
        }

        public async Task ArchivarAvisoAsync(int idAviso)
        {
            await _avisoValidator.ValidarIdAsync(idAviso);

            await _avisoRepository.CambiarStatusArchivadoAsync(idAviso, true);
        }

        public async Task DesarchivarAvisoAsync(int idAviso)
        {
            await _avisoValidator.ValidarIdAsync(idAviso);

            await _avisoRepository.CambiarStatusArchivadoAsync(idAviso, false);
        }

        public async Task<string> VerComentariosAsync(int idAviso)
        {
            await _avisoValidator.ValidarIdAsync(idAviso);

            return await _avisoRepository.VerComentariosAsync(idAviso);
        }

        public async Task PublicarAvisoAsync(int idAviso, string url)
        {
            await _avisoValidator.ValidarPublicacionAsync(idAviso, url);

            await _avisoRepository.PublicarAsync(idAviso, url);
        }

        public async Task<bool> VerificarEstadoAvisoAsync(int idAviso, string estado)
        {
            await _avisoValidator.ValidarIdAsync(idAviso);

            return await _avisoRepository.VerificarEstadoAsync(idAviso, estado);
        }

        //EA
        public async Task CrearAviso(CrearAvisoDTO aviso)
        {
            //await _avisoValidator.ValidarCrearAviso(aviso);

            DatosArchivoGuardadoDTO? archivoGuardado = null;
            Archivo? archivoRegistrado = null;
            try
            {
                /*archivoGuardado = await _archivoService.GuardarAsync(aviso.archivo.RutaArchivo, aviso.archivo.NombreArchivo, "archivos-avisos");
                archivoRegistrado = await _archivoRepository.CrearAsync(new Archivo
                {
                    Nombre = archivoGuardado.NombreOriginal,
                    Ruta = archivoGuardado.Ruta,
                    Tipo = archivoGuardado.Tipo,
                    Tamanio = archivoGuardado.Tamanio
                });*/

                var avisoRegistrado = new Aviso
                {
                    IdEntidadAcademica = aviso.IdEntidadAcademica,
                    IdPeriodo = aviso.IdPeriodo,
                    IdArticulo = aviso.IdArticulo,
                    Folio = aviso.Folio,
                    FechaCreacion = aviso.FechaCreacion,
                    FechaCt = aviso.FechaCT,
                    FechaVacantes = aviso.FechaVacantes,
                    Requisitos = aviso.Requisitos,
                    Lugar = aviso.Lugar,
                    Correo = aviso.Correo,
                    Modalidad = aviso.Modalidad,
                    Estado = "Creado",
                    Sistema = "Escolarizado"
                    //IdArchivoOriginal = archivoRegistrado.IdArchivo
                };

                avisoRegistrado.IdArchivoOriginal = await generarArchivoAvisoAsync(avisoRegistrado);

                avisoRegistrado = await _avisoRepository.CrearAsync(avisoRegistrado);
                List<Horario> horarios = new List<Horario>();
                foreach (var h in aviso.Horarios)
                {
                    horarios.Add(new Horario
                    {
                        IdAviso = avisoRegistrado.IdAviso,
                        Dia = h.Fecha,
                        HoraInicio = TimeOnly.Parse(h.HoraInicio),
                        HoraFin = TimeOnly.Parse(h.HoraTermino)
                    });
                }
                
                
                await _avisoRepository.AsociarOfertasPorAviso(aviso.OfertasId, avisoRegistrado.IdAviso);
                await _horarioRepository.CrearHorarios(horarios);

            }
            catch (Exception ex)
            {
                if (archivoRegistrado != null)
                    await _archivoRepository.EliminarAsync(archivoRegistrado);
                if (archivoGuardado != null)
                    await _archivoService.EliminarAsync(archivoGuardado.Ruta);
                throw ex;
            }
        }

        private async Task<int> generarArchivoAvisoAsync(CrearAvisoDTO aviso)
        {
            var entidad = await _entidadAcademicaRepository.ObtenerPorIdAsync(aviso.IdEntidadAcademica);
            var articulo = await _articuloRepository.ObtenerArticuloPorIdAsync(aviso.IdArticulo);
            var periodoDTO = await obtenerPeriodoDTOAsync(aviso.IdPeriodo);

            var plantillaDTO = new PlantillaAvisoDTO
            {
                Folio = aviso.Folio ?? "0",
                AreaAcademica = entidad?.IdAreaAcademicaNavigation.Nombre ?? "Nombre del Área Académica",
                EntidadAcademica = entidad?.Nombre.Substring(6) ?? "Nombre de la Entidad Académica",
                Articulo = articulo?.Numero ?? "Número del Artículo",
                Region = entidad?.Region ?? "Región",
                PerfilArticulo = articulo?.Descripcion ?? "Perfil del Artículo",
                Periodo = periodoDTO.PeriodoMostrar ?? "Periodo Escolar",
                Campus = "Campus", //TODO: El campus depende del Programa Educativo
                Sistema = aviso.Sistema ?? "Sistema", //TODO: falta obtenerlo del formulario
                Programas = await generarListaProgramasPlantillaAsync(aviso.OfertasId),
                Requisitos = aviso.Requisitos ?? "Requisitos",
                HorarioAceptacion = generarHorarioAceptacion(aviso.Horarios) ?? "Horario de Aceptación", //Se puede mejorar para que también detecte cuando un sólo día tiene dos distintos horarios
                FechaConsejoTecnico = generarFechaNormal(aviso.FechaCT) ?? "Fecha del Consejo Tecnico",
                FechaPublicacion = generarFechaNormal(DateOnly.FromDateTime(DateTime.Now)) ?? "Fecha de publicación",
                Titular = "Titular" //TODO: falta obtenerlo del formulario
            };

            return await _plantillaService.GenerarAvisoAsync(plantillaDTO);
        }

        private async Task<List<PlantillaAvisoProgramaEducativoDTO>> generarListaProgramasPlantillaAsync(List<int> ofertasId)
        {
            var listaProgramas = new List<PlantillaAvisoProgramaEducativoDTO>();
            var nombresProgramas = new List<string>();

            foreach (var id in ofertasId)
            {
                var oferta = await _ofertaRepository.ObtenerPorIdAsync(id);
                if (oferta is null) continue;

                var programa = oferta.IdProgramaEducativoNavigation;
                if (!nombresProgramas.Contains(programa.Nombre))
                {
                    var programaPlantilla = new PlantillaAvisoProgramaEducativoDTO
                    {
                        ProgramaEducativo = programa.Nombre,
                        Experiencias = [await crearExperienciaPlantillaAsync(oferta)]
                    };

                    nombresProgramas.Add(programa.Nombre);
                }
                else
                {
                    var programaPlantilla = listaProgramas.FirstOrDefault(lp => lp.ProgramaEducativo.Contains(programa.Nombre));
                    if (programaPlantilla is null) continue;

                    programaPlantilla.Experiencias.Add(await crearExperienciaPlantillaAsync(oferta));
                }
            }

            return listaProgramas;
        }

        private async Task<PlantillaAvisoExperienciaEducativaDTO> crearExperienciaPlantillaAsync(Oferta oferta)
        {
            var experiencia = oferta.IdExperienciaEducativaNavigation;
            var horarios = await _horarioRepository.ObtenerPorIdOferta(oferta.IdOferta);

            return new PlantillaAvisoExperienciaEducativaDTO
            {
                Horas = experiencia.Horas,
                Nombre = experiencia.Nombre,
                NRC = oferta.Nrc,
                Plaza = oferta?.Plaza ?? "Plaza",
                HorarioLunes = generarHorarioExperienciaPlantilla(horarios.FirstOrDefault(h => h.Dia.Contains("Lunes"))),
                HorarioMartes = generarHorarioExperienciaPlantilla(horarios.FirstOrDefault(h => h.Dia.Contains("Martes"))),
                HorarioMiercoles = generarHorarioExperienciaPlantilla(horarios.FirstOrDefault(h => h.Dia.Contains("Miercoles"))),
                HorarioJueves = generarHorarioExperienciaPlantilla(horarios.FirstOrDefault(h => h.Dia.Contains("Jueves"))),
                HorarioViernes = generarHorarioExperienciaPlantilla(horarios.FirstOrDefault(h => h.Dia.Contains("Viernes"))),
                HorarioSabado = generarHorarioExperienciaPlantilla(horarios.FirstOrDefault(h => h.Dia.Contains("Sabado"))),
                TipoContratacion = oferta?.TipoContratacion ?? "Tipo de Contratación",
                PerfilDocente = experiencia.PerfilDocente ?? "Perfil del docente"
            };
        }

        private string generarHorarioExperienciaPlantilla(Horario? horario)
        {
            if (horario is null) return "";

            return $"{horario.HoraInicio} - {horario.HoraFin}";
        }

        private string generarHorarioAceptacion(List<CrearHorarioAvisoDTO> horariosDTO)
        {
            var cadena = "";
            var listaMeses = new List<int>();
            var listaDates = new List<DateTime>();

            foreach (var horario in horariosDTO)
            {
                if (DateTime.TryParse(horario.Fecha, out DateTime fecha))
                {
                    listaDates.Add(fecha);
                    var mes = fecha.Month;
                    if (!listaMeses.Contains(mes)) listaMeses.Add(mes);
                }
            }

            foreach (var mes in listaMeses)
            {
                var cadenaActual = "";

                foreach (var horario in horariosDTO)
                {
                    if (DateTime.TryParse(horario.Fecha, out DateTime fecha))
                    {
                        if (fecha.Month == mes)
                        {
                            var horarioDia = $"{fecha.Day} de {horario.HoraInicio} a {horario.HoraTermino}, ";
                            cadenaActual = $"{cadenaActual}{horarioDia}";
                        }
                    }
                }

                var nombreMes = CultureInfo.GetCultureInfo("es-ES").DateTimeFormat.GetMonthName(mes);
                cadena = $"{cadenaActual}de {nombreMes}, ";
            }

            return cadena;
        }

        private string generarFechaNormal(DateOnly fecha)
        {
            return fecha.ToString("d 'de' MMM 'de' yyyy", new CultureInfo("es-ES"));
        }

        public async Task EliminarAvisoPorId(int idAviso)
        {
            await _avisoValidator.ValidarIdAsync(idAviso);

            await _avisoRepository.EliminarAsync(idAviso);
        }

        public Task ActualizarAvisoPorId(EditarAvisoDTO aviso)
        {
            throw new NotImplementedException();
        }

        public async Task EnviarARevisionAsync(RevisionDTO revisionDTO)
        {
            await _avisoValidator.ValidarEnviarARevisionAsync(revisionDTO);

            await _avisoRepository.EnviarARevisionAsync(revisionDTO.IdAviso, revisionDTO.Comentarios);
        }

        public async Task FirmarAvisoAsync(int idAviso, CargarArchivoDTO archivoDTO)
        {
            await _avisoValidator.ValidarIdAsync(idAviso);
            await _avisoValidator.ValidarArchivoAsync(archivoDTO);

            DatosArchivoGuardadoDTO? archivoDisco = null;
            Archivo? archivo = null;
            Archivo? archivoGuardado = null;

            try
            {
                archivoDisco = await _archivoService.GuardarAsync(archivoDTO.RutaArchivo, archivoDTO.NombreArchivo, "aviso-firmado");
                archivo = new Archivo
                {
                    Nombre = archivoDisco.NombreOriginal,
                    Ruta = archivoDisco.Ruta,
                    Tipo = archivoDisco.Tipo,
                    Tamanio = archivoDisco.Tamanio
                };
                archivoGuardado = await _archivoRepository.CrearAsync(archivo);

                await _avisoRepository.FirmarAsync(idAviso, archivoGuardado.IdArchivo);
            }
            catch (Exception ex)
            {
                if (archivoGuardado is null)
                    await _archivoService.EliminarAsync(archivo?.Ruta ?? "");

                throw;
            }
        }


        //DGAA
        public async Task RevisarAvisoAsync(RevisionDTO revisionDTO)
        {
            throw new NotImplementedException();
        }


        //Utils
        private async Task<ListaAvisosDTO> mapearAListaAvisoDTOAsync(Aviso aviso)
        {
            var periodoDTO = await obtenerPeriodoDTOAsync(aviso.IdPeriodo);
            var avisoDTO = new ListaAvisosDTO
            {
                IdAviso = aviso.IdAviso,
                IdEntidadAcademica = aviso.IdEntidadAcademica,
                IdPeriodo = aviso.IdPeriodo,
                IdArticulo = aviso.IdArticulo,
                NombreEntidadAcademica = aviso.IdEntidadAcademicaNavigation.Nombre,
                Folio = aviso.Folio,
                Periodo = periodoDTO.PeriodoMostrar,
                Articulo = aviso.IdArticuloNavigation.Numero,
                FechaCreacion = aviso.FechaCreacion.ToString("dd/MM/yyyy"),
                Estado = aviso.Estado,
                Archivado = aviso.Archivado ?? false,
                Comentarios = aviso.Comentarios ?? ""
            };

            return avisoDTO;
        }

        private async Task<DetallesPeriodoEscolarDTO> obtenerPeriodoDTOAsync(int idPeriodo)
        {
            var periodo = await _periodoEscolarRepository.ObtenerPorIdAsync(idPeriodo);
            var periodoDTO = PeriodoEscolarMapper.ToDTO(periodo);
            return periodoDTO;
        }

        private async Task<List<OfertaAvisoDTO>> obtenerOfertasDeAvisoAsync(int idAviso)
        {
            var ofertas = await _ofertaRepository.ObtenerPorAvisoAsync(idAviso);
            var ofertasDTO = new List<OfertaAvisoDTO>();
            foreach (var o in ofertas)
            {
                var ofertaDTO = new OfertaAvisoDTO
                {
                    NombrePlanEstudio = o.IdExperienciaEducativaNavigation.IdPlanEstudiosNavigation.Nombre,
                    Horas = "" + o.Hsm,
                    NombreExperienciaEducativa = o.IdExperienciaEducativaNavigation.Nombre,
                    NRC = o.Nrc,
                    Plaza = o.Plaza ?? "",
                    TipoPlaza = o.TipoPlaza ?? "",
                    TipoContratacion = o.TipoContratacion ?? "",
                    PerfilDocente = o.IdExperienciaEducativaNavigation.PerfilDocente,
                };

                var dias = o.Horario;
                foreach (var d in dias)
                {
                    var horarioDTO = new HorarioDia
                    {
                        Inicio = d.HoraInicio.ToTimeSpan(),
                        Fin = d.HoraFin.ToTimeSpan()
                    };
                    switch (d.Dia)
                    {
                        case "Lunes":
                            ofertaDTO.Lunes = horarioDTO;
                            break;
                        case "Martes":
                            ofertaDTO.Martes = horarioDTO;
                            break;
                        case "Miércoles":
                            ofertaDTO.Martes = horarioDTO;
                            break;
                        case "Jueves":
                            ofertaDTO.Jueves = horarioDTO;
                            break;
                        case "Viernes":
                            ofertaDTO.Viernes = horarioDTO;
                            break;
                    }
                }

                ofertasDTO.Add(ofertaDTO);
            }

            return ofertasDTO;
        }

        private async Task<DatosHorarioDTO> obtenerHorarioAvisoAsync(int idAviso)
        {
            var horarios = await _horarioRepository.ObtenerPorIdAviso(idAviso);
            var horarioDTO = new DatosHorarioDTO
            {
                IdAviso = idAviso
            };
            foreach (var h in horarios)
            {
                var diaDTO = new DiaDTO
                {
                    Dia = h.Dia,
                    HoraFin = h.HoraFin.ToTimeSpan(),
                    HoraInicio = h.HoraInicio.ToTimeSpan()
                };
                horarioDTO.Dias.Add(diaDTO);
            }

            return horarioDTO;
        }
        //Datos necesarios
        public async Task<List<OfertaPlanEstudiosAvisoDTO>> ObtenerPlanesConOfertasAviso(int idEntidadAcademica, int idPeriodo, int idArticulo)
        {
            var planes = await _ofertaRepository.ObtenerPlanesEstudioCrearAviso(idEntidadAcademica, idPeriodo, idArticulo);

            return planes;
        }
    }
}
