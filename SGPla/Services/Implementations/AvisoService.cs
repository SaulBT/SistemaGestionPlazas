using SGPla.Mappers;
using SGPla.Models;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Aviso;
using SGPla.Models.DTOs.Horario;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.PeriodoEscolar;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Services.Implementations
{
    public class AvisoService : IAvisoService
    {
        private readonly IAvisoRepository _avisoRepository;
        private readonly IPeriodoEscolarRepository _periodoEscolarRepository;
        private readonly IOfertaRepository _ofertaRepository;
        private readonly IHorarioRepository _horarioRepository;
        private readonly IArchivoService _archivoService;
        private readonly IArchivoRepository _archivoRepository;
        private readonly IAvisoValidator _avisoValidator;

        public AvisoService(
            IAvisoRepository avisoRepository,
            IPeriodoEscolarRepository periodoEscolarRepository,
            IOfertaRepository ofertaRepository,
            IHorarioRepository horarioRepository,
            IArchivoService archivoService,
            IArchivoRepository archivoRepository,
            IAvisoValidator avisoValidator)
        {
            _avisoRepository = avisoRepository;
            _periodoEscolarRepository = periodoEscolarRepository;
            _ofertaRepository = ofertaRepository;
            _horarioRepository = horarioRepository;
            _archivoService = archivoService;
            _archivoRepository = archivoRepository;
            _avisoValidator = avisoValidator;
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
                FechaVacantes = aviso.FechaInicio.ToString("dd/MM/yyyy"),
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
        public Task CrearAviso(CrearAvisoDTO aviso)
        {
            throw new NotImplementedException();
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
                Estado = aviso.Estado
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
    }
}
