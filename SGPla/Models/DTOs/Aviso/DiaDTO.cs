namespace SGPla.Models.DTOs.Aviso
{
    public class DiaDTO
    {
        public string Dia { get; set; } = string.Empty;
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
    }
}
