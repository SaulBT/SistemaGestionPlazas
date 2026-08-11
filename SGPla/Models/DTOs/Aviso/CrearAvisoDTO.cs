using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Horario;

namespace SGPla.Models.DTOs.Aviso
{
    public class CrearAvisoDTO
    {
        //public int IdAviso { get; set; }
        public int IdEntidadAcademica { get; set; }
        public int IdPeriodo { get; set; }
        public int IdArticulo { get; set; }
        public string Folio { get; set; }
        public DateOnly FechaCreacion{ get; set; } 
        public DateOnly FechaCT { get; set; } 
        public DateOnly FechaVacantes { get; set; }
        public string Requisitos { get; set; }
        public string Lugar { get; set; }
        public string Correo { get; set; }
        public string Modalidad { get; set; }
        //public int IdArchivoOriginal { get; set; }
        public CargarArchivoDTO archivo { get; set; } = new CargarArchivoDTO();
        //public int IdArchivoFirmado { get; set; }
        //public bool Archivado { get; set; }
        //public string NombreTitular { get; set; }
        public List<CrearHorarioAvisoDTO> Horarios { get; set; } = new List<CrearHorarioAvisoDTO>();
        public List<int> OfertasId { get; set; } = new List<int>();
    }
}
