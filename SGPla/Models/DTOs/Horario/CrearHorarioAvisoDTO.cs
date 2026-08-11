namespace SGPla.Models.DTOs.Horario
{
    public class CrearHorarioAvisoDTO
    {
        public int IdAviso { get; set; }
        public string Fecha { get; set; }
        public string HoraInicio { get; set; }
        public string HoraTermino { get; set; }
        public int IdTemporal { get; set; }
    }
}
