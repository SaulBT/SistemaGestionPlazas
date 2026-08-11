using SGPla.Models.DTOs.Aviso;
using SGPla.Services.Implementations;

namespace SGPla.Models.DTOs.Horario
{
    public class DatosHorarioDTO
    {
        public int IdAviso { get; set; }
        public int IdOferta { get; set; }
        public string Dia { get; set; } = string.Empty;
        public string salon {  get; set; } = string.Empty;
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public List<DiaDTO> Dias { get; set; } = [];
    }
}
