using SGPla.Models.DTOs.Horario;

namespace SGPla.Models.DTOs.Oferta
{
    public class DatosOfertaAvisoDTO
    {
        public int IdOferta { get; set; }
        public int Horas { get; set; }
        public string ExperienciaEducativa { get; set; }
        public string NRC { get; set; }
        public string Plaza { get; set; }
        public string TipoContratacion { get; set; }
        public string PerfilDocente { get; set; }
        public List<DatosHorarioAvisoDTO> Horarios { get; set; }
    }
}
