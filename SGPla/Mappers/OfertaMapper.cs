using SGPla.Models;
using SGPla.Models.DTOs.Oferta;

namespace SGPla.Mappers
{
    public class HorarioMapper
    {
        public static ICollection<Horario> ToModel(IEnumerable<(string Dia, HorarioDia Horario)> horario)
        {
            var model = new List<Horario>();
            foreach (var (dia, horarioDia) in horario)
            {
                var horarioModel = new Horario
                {
                    Dia = dia,
                    HoraInicio = TimeOnly.FromTimeSpan(horarioDia.Inicio),
                    HoraFin = TimeOnly.FromTimeSpan(horarioDia.Fin)
                };
                model.Add(horarioModel);
            }
            return model;
        }
    }


    public class OfertaMapper
    {
        

        public static Oferta ToModel(OfertaDTO dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto), "El DTO proporcionó datos nulos.");

            return new Oferta
            {
                Horario = HorarioMapper.ToModel(dto.DiasConClase()),
                Nrc = dto.NRC,
                Plaza = dto.Plaza,
                TipoContratacion = dto.TC,
                Incluida = dto.Incluida,
                Hsm = dto.HorasPago,
                EstadoSolicitudApertura = "Aceptada",
                IdArticulo = dto.IdArticulo,
                IdPeriodo = dto.IdPeriodo,
                IdProgramaEducativo = dto.IdProgramaEducativo
            };
        }

       
    }
}
