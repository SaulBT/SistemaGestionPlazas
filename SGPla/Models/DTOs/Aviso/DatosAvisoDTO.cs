using SGPla.Models.DTOs.Horario;
using SGPla.Models.DTOs.Oferta;

namespace SGPla.Models.DTOs.Aviso
{
    public class DatosAvisoDTO
    {
        public int IdAviso { get; set; }
        public int IdEntidadAcademica { get; set; }
        public int IdPeriodo { get; set; }
        public int IdArticulo { get; set; }
        public int IdArchivoOriginal { get; set; }
        public int IdArchivoFirmado { get; set; }
        public string Articulo { get; set; } = string.Empty;
        public string FechaCreacion { get; set; } = string.Empty;
        public string FechaVacantes { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string NombreEntidadAcademica { get; set; } = string.Empty;
        public string Periodo { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;
        public string Sistema { get; set; } = string.Empty;
        public List<OfertaAvisoDTO> Ofertas { get; set; } = [];
        public string Requisitos { get; set; } = string.Empty;
        public string Modalidad { get; set; } = string.Empty;
        public string Lugar { get; set; } = string.Empty;
        public DatosHorarioDTO Horario { get; set; } = new DatosHorarioDTO();
        public string UrlPublicacion { get; set; } = string.Empty;
        public List<DatosHorarioDTO> Horarios { get; set; } = new List<DatosHorarioDTO>();
    }
}
