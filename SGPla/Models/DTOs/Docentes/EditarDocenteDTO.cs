using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Grados;

namespace SGPla.Models.DTOs.Docentes
{
    public class EditarDocenteDTO
    {
        public int IdDocente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string DescripcionPerfil { get; set; } = string.Empty;
        public string NumeroPersonal { get; set; } = string.Empty;
        public string Puesto { get; set; } = string.Empty;
        public List<AgregarGradoDTO> GradosAgregados = new List<AgregarGradoDTO>();
        public List<DatosGradoDTO> GradosEditados = new List<DatosGradoDTO>();
        public List<int> IdsGradosEliminados = new List<int>();
        public bool NuevoArchivo { get; set; }
        public CargarArchivoDTO ArchivosGenerales { get; set; } = new CargarArchivoDTO();
    }
}
