using SGPla.Models.DTOs.Grados;

namespace SGPla.Models.DTOs.Docentes
{
    public class DatosDocenteDTO
    {
        public int IdDocente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string DescripcionPerfil { get; set; } = string.Empty;
        public int IdArchivosGenerales { get; set; }
        public List<DatosGradoDTO> Grados { get; set; } = new List<DatosGradoDTO>();
        public string NumeroPersonal { get; set; } = string.Empty;
        public string Puesto { get; set; } = string.Empty;
    }
}
