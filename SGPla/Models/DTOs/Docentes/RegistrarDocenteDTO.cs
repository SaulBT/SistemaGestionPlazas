using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Grados;

namespace SGPla.Models.DTOs.Docentes
{
    public class RegistrarDocenteDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string DescripcionPerfil { get; set; } = string.Empty;
        public CargarArchivoDTO ArchivosGenerales { get; set; } = new CargarArchivoDTO();
        public List<AgregarGradoDTO> Grados { get; set; } = new List<AgregarGradoDTO>();
        public string NumeroPersonal { get; set; } = string.Empty;
        public string Puesto { get; set; } = string.Empty;
    }
}
