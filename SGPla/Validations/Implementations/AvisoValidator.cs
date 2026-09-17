using SGPla.Commons;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Aviso;
using SGPla.Models.DTOs.Horario;
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
                if (string.IsNullOrWhiteSpace(revisionDTO.Comentarios))
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

        public Task ValidarCrearAviso(CrearAvisoDTO dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            validarDatosAviso(dto);
            validarHorarios(dto.Horarios);
            return Task.CompletedTask;
        }

        private void validarDatosAviso(CrearAvisoDTO dto)
        {
            if (dto.IdEntidadAcademica <= 0 || dto.IdPeriodo <= 0 || dto.IdArticulo <= 0 ||
                dto.FechaCreacion == DateOnly.MinValue || dto.FechaPublicacion == DateOnly.MinValue ||
                dto.FechaCT == DateOnly.MinValue || dto.FechaVacantes == DateOnly.MinValue ||
                string.IsNullOrWhiteSpace(dto.Requisitos) || string.IsNullOrWhiteSpace(dto.Modalidad) ||
                string.IsNullOrWhiteSpace(dto.Correo))
                throw new ValidacionExcepction("Hay campos obligatorios sin completar.", "400");

            if (!System.Net.Mail.MailAddress.TryCreate(dto.Correo, out _))
                throw new ValidacionExcepction("El correo electrónico no es válido.", "400");

            if (dto.Modalidad != Constantes.MODALIDAD_AVISO_PRESENCIAL && dto.Modalidad != Constantes.MODALIDAD_AVISO_VIRTUAL)
                throw new ValidacionExcepction("La modalidad no es válida.", "400");

            if (dto.Modalidad == Constantes.MODALIDAD_AVISO_PRESENCIAL && string.IsNullOrWhiteSpace(dto.Lugar))
                throw new ValidacionExcepction("El lugar es obligatorio para la modalidad presencial.", "400");

            if (dto.OfertasId is null || dto.OfertasId.Count == 0)
                throw new ValidacionExcepction("Seleccione al menos una oferta.", "400");

            if (dto.OfertasId.Distinct().Count() != dto.OfertasId.Count)
                throw new ValidacionExcepction("No es posible seleccionar una oferta más de una vez.", "400");
        }

        private void validarHorarios(List<CrearHorarioAvisoDTO> horarios)
        {
            if (horarios is null || horarios.Count == 0)
                throw new ValidacionExcepction("Llena la tabla de Horarios.", "400");

            if (horarios.Any(h => h is null || !DateOnly.TryParse(h.Fecha, out _) ||
                !TimeOnly.TryParse(h.HoraInicio, out var inicio) ||
                !TimeOnly.TryParse(h.HoraTermino, out var fin) || inicio >= fin))
                throw new ValidacionExcepction("Cada horario debe tener una fecha y hora de inicio menor a la hora de término.", "400");
        }
    }
}
