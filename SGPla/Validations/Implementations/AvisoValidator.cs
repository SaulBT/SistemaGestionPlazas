using SGPla.Commons;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Aviso;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Validations.Implementations
{
    public class AvisoValidator : IAvisoValidator
    {
        private readonly IAvisoRepository _avisoRepository;

        public AvisoValidator(IAvisoRepository avisoRepository)
        {
            _avisoRepository = avisoRepository;
        }

        public async Task ValidarIdAsync(int idAviso)
        {
            if (idAviso <= 0)
                throw new ValidacionExcepction("La Id del Aviso es inválida.", "400");
            if (!await _avisoRepository.ExistePorId(idAviso))
                throw new ValidacionExcepction("No existe ese Aviso", "404");
        }

        public async Task ValidarEnviarARevisionAsync(RevisionDTO revisionDTO)
        {
            if (revisionDTO is null)
                throw new ValidacionExcepction("No se enviaron datos", "400");
            else
            {
                await ValidarIdAsync(revisionDTO.IdAviso);
                if (string.IsNullOrEmpty(revisionDTO.Comentarios))
                    throw new ValidacionExcepction("Los Comentarios son obligatorios.", "400");
            }
        }

        public async Task ValidarArchivoAsync(CargarArchivoDTO cargarArchivoDTO)
        {
            if (cargarArchivoDTO is null)
                throw new ValidacionExcepction("No se envió el archivo", "400");
            else
            {
                if (string.IsNullOrEmpty(cargarArchivoDTO.NombreArchivo))
                    throw new ValidacionExcepction("El Nombre del Archivo es obligatorio", "400");
                if (string.IsNullOrEmpty(cargarArchivoDTO.RutaArchivo))
                    throw new ValidacionExcepction("La Ruta del Archivo es obligatoria", "400");
            }
        }

        public async Task ValidarPublicacionAsync(int idAaviso, string url)
        {
            await ValidarIdAsync(idAaviso);
            if (string.IsNullOrEmpty(url))
                throw new ValidacionExcepction("La URL de la publicación es obligatoria", "400");
        }
    }
}
