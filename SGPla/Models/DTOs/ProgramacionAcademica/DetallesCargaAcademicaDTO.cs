namespace SGPla.Models.DTOs.ProgramacionAcademica
{
    public class DetallesCargaAcademicaDTO
    {
        public string NP { get; set; }

        public string Docente { get; set; }

        
        public string Plaza { get; set; }

        public string NRC { get; set; }

        public string ExperienciaEducativa { get; set; }

        public string HorasContacto { get; set; }

        public string HorasPago { get; set; }

        public bool? Imparte {  get; set; }
    }
}
