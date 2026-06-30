namespace SGPla.Models.DTOs.ProgramacionAcademica
{
    public class CargaConOfertaDTO
    {
        public string NumeroPersonal { get; set; } = string.Empty;
        public string NombreDocente { get; set; } = string.Empty;
        public string? Plaza { get; set; } = string.Empty;
        public string? Categoria { get; set; } = string.Empty;
        public string? TipoContratacion { get; set; } = string.Empty;
        public string? Nrc { get; set; } = string.Empty;
        public string ExperienciaEducativa { get; set; } = string.Empty;
        public int HorasContacto { get; set; }
        public int HorasPago { get; set; }
        public string MotivoRh { get; set; } = string.Empty;
        public string IndActDocente { get; set; } = string.Empty;
        public bool? Imparte { get; set; }
        public string? Programa { get; set; }
        public string? NpOferta { get; set; }   
        public string? DocenteOferta { get; set; }
        public bool NrcEncontrado { get; set; }

        public int idPeriodo { get; set; }

        public int? idExperienciaEducativa { get; set; }

        public int idDocente { get; set; }
    }

    public class CargaItemDTO
    {
        public string Nrc { get; set; } = string.Empty;

        public string ExperienciaEducativa { get; set; } = string.Empty;

        public int HorasContacto { get; set; }

        public int HorasPago { get; set; }

        public string ClaveProgramatica { get; set; } = string.Empty;

        public string MotivoRh { get; set; } = string.Empty;

        public string IndActDocente { get; set; } = string.Empty;

        public bool? Imparte { get; set; }

        public int HorasExcCarga { get; set; }

        public string? NumPersonalSuplente { get; set; }

        public string? NombreSuplente { get; set; }

        public string Plaza { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Puesto { get; set; } = string.Empty;
        public string TipoContratacion { get; set; } = string.Empty;
    }


    public class DocenteCargaDTO
    {
        public string NumeroPersonal { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Antiguedad { get; set; } = string.Empty;

        public string Plaza { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public string Puesto { get; set; } = string.Empty;

        public string TipoContratacion { get; set; } = string.Empty;

        public int TotalHoras { get; set; }

        public List<CargaItemDTO> Experiencias { get; set; } = new();
    }


    public class CargasAcademicasDTO
    {
        public string Periodo { get; set; } = string.Empty;

        public List<DocenteCargaDTO> Docentes { get; set; } = new();
    }
}
