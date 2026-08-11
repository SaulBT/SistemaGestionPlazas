namespace SGPla.Models.DTOs.Plantillas
{
    public class PlantillaAvisoDTO
    {
        public string Folio { get; set; } = "0";
        public string AreaAcademica { get; set; } = string.Empty;
        public string EntidadAcademica { get; set; } = string.Empty;
        public string Articulo { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Periodo { get; set; } = string.Empty;
        public string Campus { get; set; } = string.Empty;
        public string Sistema { get; set; } = string.Empty;
        public string ProgramaEducativo { get; set; } = string.Empty;
        public List<PlantillaAvisoExperienciaEducativaDTO> ListaExperiencias { get; set; } = [];
        public string Requisitos { get; set; } = string.Empty;
        public string DiasAceptacion { get; set; } = string.Empty;
        public string FechaConsejoTecnico { get; set; } = string.Empty;
        public string FechaPublicacion { get; set; } = string.Empty;
        public string Titular { get; set; } = "Nombre del Titular";
    }

    public class PlantillaAvisoExperienciaEducativaDTO
    {
        public string Horas { get; set;  } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string NRC { get; set; } = string.Empty;
        public string Plaza { get; set; } = string.Empty;
        public string HorarioLunes { get; set; } = string.Empty;
        public string HorarioMartes { get; set; } = string.Empty;
        public string HorarioMiercoles { get; set; } = string.Empty;
        public string HorarioJueves { get; set; } = string.Empty;
        public string HorarioViernes { get; set; } = string.Empty;
        public string HorarioSabado { get; set; } = string.Empty;
        public string TipoContratacion { get; set; } = string.Empty;
        public string PerfilDocente { get; set; } = string.Empty;
    }
}
