using SGPla.Models;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Aviso;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using SGPla.Validations.Interfaces;

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

        public AvisoService(
            IAvisoRepository avisoRepository, 
            IProgramacionAcademicaRepository programacionAcademicaRepository, 
            IOfertaRepository ofertaRepository,
            IArchivoRepository archivoRepository,
            IArchivoService archivoService,
            IHorarioRepository horarioRepository,
            IAvisoValidator avisoValidator)
        {
            _avisoRepository = avisoRepository;
            _programacionAcademicaRepository = programacionAcademicaRepository;
            _ofertaRepository = ofertaRepository;
            _archivoRepository = archivoRepository;
            _archivoService = archivoService;
            _horarioRepository = horarioRepository;
            _avisoValidator = avisoValidator;
        }

        public async Task<(List<ListaAvisosDTO> items, int total)> ObtenerTodosAvisosAsync(FiltroAvisosDTO filtroDTO)
        {
            throw new NotImplementedException();
        }

        public Task<DatosAvisoDTO> ObtenerAvisoPorIDAsync(int idAviso)
        {
            throw new NotImplementedException();
        }

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
                    //IdArchivoOriginal = archivoRegistrado.IdArchivo
                };

                avisoRegistrado = await _avisoRepository.CrearAviso(avisoRegistrado);
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

        public Task EliminarAvisoPorId(int idAviso)
        {
            throw new NotImplementedException();
        }

        public Task ActualizarAvisoPorId(EditarAvisoDTO aviso)
        {
            throw new NotImplementedException();
        }

        //DGAA
        public async Task RevisarAvisoAsync(RevisionDTO revisionDTO)
        {
            throw new NotImplementedException();
        }

        public async Task ArchivarAvisoAsync(int idAviso)
        {
            throw new NotImplementedException();
        }

        //Datos necesarios
        public async Task<List<OfertaPlanEstudiosAvisoDTO>> ObtenerPlanesConOfertasAviso(int idEntidadAcademica, int idPeriodo, int idArticulo)
        {
            var planes = await _ofertaRepository.ObtenerPlanesEstudioCrearAviso(idEntidadAcademica, idPeriodo, idArticulo);

            return planes;
        }
    }
}
