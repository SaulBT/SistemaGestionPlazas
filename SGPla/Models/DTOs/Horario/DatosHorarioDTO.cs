namespace SGPla.Models.DTOs.Horario
{
    public class DatosHorarioDTO
    {
        public int IdHorario { get; set; }
        public int IdAviso { get; set; }
        public List<DiaDTO> Dias { get; set; } = [];
    }

    public class DiaDTO
    {
        public string Dia { get; set; } = string.Empty;
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
    }
}
