using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
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

        public async Task<Oferta?> ObtenerPorIdAsync(int id)
        {
            return _context.Oferta
                .Include(o => o.IdProgramaEducativoNavigation)
                .Include(o => o.IdExperienciaEducativa)
                .FirstOrDefault(o => o.IdOferta == id);
        }

        public async Task<List<Oferta>> ObtenerPorAvisoAsync(int idAviso)
        {
            var idsOferta = await _context.OfertaAviso
                .Where(oa => oa.IdAviso == idAviso)
                .Select(oa => oa.IdOferta)
                .ToListAsync();

            var ofertas = new List<Oferta>();
            foreach (var id in idsOferta)
            {
                if (id > 0)
                {
                    var oferta = await _context.Oferta
                        .Include(o => o.IdExperienciaEducativaNavigation)
                        .Include(o => o.IdExperienciaEducativaNavigation.IdPlanEstudiosNavigation)
                        .FirstOrDefaultAsync(o => o.IdOferta == id);
                    if (oferta is not null)
                        ofertas.Add(oferta);
                }
            }

            return ofertas;
        }
        public async Task<List<OfertaPlanEstudiosAvisoDTO>> ObtenerPlanesEstudioCrearAviso(int idEntidadAcademica, int idPeriodo, int idArticulo)
        {
            /* 
             *      Actualmente el método retorna Programas Educativos, no planes de estudio
             *      Debido a que todavía no hay forma de vincular las ofertas a los planes
             */
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
                        //+ ": " +
                        //o.IdExperienciaEducativaNavigation.IdPlanEstudiosNavigation.Nombre
                        //Comentado para no mostrar el plan de estudios
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
