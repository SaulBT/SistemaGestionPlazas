using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models.DTOs.Horario;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public class OfertaRepository : IOfertaRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public OfertaRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<List<OfertaPlanEstudiosAvisoDTO>> ObtenerPlanesEstudioCrearAviso(int idEntidadAcademica, int idPeriodo, int idArticulo)
        {
            List<OfertaPlanEstudiosAvisoDTO> planesDTO = new List<OfertaPlanEstudiosAvisoDTO>();

            var resultados = await _context.Oferta
                .Include(o => o.IdExperienciaEducativaNavigation)
                .Include(o => o.Horario)
                .Include(o => o.IdProgramaEducativoNavigation)
                .Where(o => o.Incluida 
                    && o.IdProgramaEducativoNavigation.IdEntidadAcademica == idEntidadAcademica
                    && o.IdPeriodo == idPeriodo
                    && o.IdArticulo == idArticulo)
                .GroupBy(o => new
                {
                    o.IdExperienciaEducativaNavigation.IdPlanEstudios,
                    Nombre = o.IdExperienciaEducativaNavigation.IdPlanEstudiosNavigation.IdProgramaEducativoNavigation.Nombre
                        + ": " +
                        o.IdExperienciaEducativaNavigation.IdPlanEstudiosNavigation.Nombre
                })
                .Select(g => new
                {
                    idPlanEstudios = g.Key.IdPlanEstudios,
                    nombre = g.Key.Nombre,
                    Ofertas = g.ToList()
                })
                .ToListAsync();

            foreach (var r in resultados)
            {
                List<DatosOfertaAvisoDTO> ofertas = new List<DatosOfertaAvisoDTO> ();

                foreach (var o in r.Ofertas)
                {
                    List<DatosHorarioAvisoDTO> horarios = new List<DatosHorarioAvisoDTO>();

                    foreach(var h in o.Horario)
                    {
                        DatosHorarioAvisoDTO horario = new DatosHorarioAvisoDTO
                        {
                            Dia = h.Dia,
                            Hora = h.HoraInicio + " - " + h.HoraFin,
                            Salon = h.Salon
                        };
                        horarios.Add(horario);
                    }

                    DatosOfertaAvisoDTO oferta = new DatosOfertaAvisoDTO
                    {
                        IdOferta = o.IdOferta,
                        Horas = o.Hsm,
                        ExperienciaEducativa = o.IdExperienciaEducativaNavigation.Nombre,
                        NRC = o.Nrc,
                        Plaza = o.Plaza,
                        TipoContratacion = o.TipoContratacion,
                        PerfilDocente = o.IdExperienciaEducativaNavigation.PerfilDocente,
                        Horarios = horarios,
                    };
                    ofertas.Add(oferta);
                }

                OfertaPlanEstudiosAvisoDTO plan = new OfertaPlanEstudiosAvisoDTO
                {
                    Nombre = r.nombre,
                    Ofertas = ofertas
                };
                planesDTO.Add(plan);
            }

            return planesDTO;
        }
    }
}
