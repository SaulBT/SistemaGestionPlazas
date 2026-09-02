namespace SGPla.Models.DTOs.Oferta
{
    public class OfertaAvisoDTO
    {
        public string NombrePlanEstudio = string.Empty;
        public string Horas { get; set; } = string.Empty;
        public string NombreExperienciaEducativa { get; set; } = string.Empty;
        public string NRC { get; set; } = string.Empty;
        public string Plaza { get; set; } = string.Empty;
        public string TipoPlaza { get; set; } = string.Empty;
        public string TipoContratacion { get; set; } = string.Empty;
        public string PerfilDocente { get; set; } = string.Empty;

        //Horario
        public HorarioDia? Lunes { get; set; }
        public HorarioDia? Martes { get; set; }
        public HorarioDia? Miercoles { get; set; }
        public HorarioDia? Jueves { get; set; }
        public HorarioDia? Viernes { get; set; }
    }
}
