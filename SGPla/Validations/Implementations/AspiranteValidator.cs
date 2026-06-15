using SGPla.Commons;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Docentes;
using SGPla.Models.DTOs.Grados;
using SGPla.Repositories.Implementations;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Validations.Implementations
{
    public class AspiranteValidator : IDocenteValidator
    {
        private readonly IAspiranteRepository _aspiranteRepository;
        private readonly IGradoRepository _gradoRepository;

        public AspiranteValidator(IAspiranteRepository aspiranteRepository, IGradoRepository gradoRepository)
        {
            _aspiranteRepository = aspiranteRepository;
            _gradoRepository = gradoRepository;
        }

        public async Task ValidarRegistroAsync(RegistrarDocenteDTO dto)
        {
            if (dto != null)
            {
                validarNombre(dto.Nombre);
                validarArchivosGenerales(dto.ArchivosGenerales);
                await validarGradosAgregadosRegistro(dto.Grados);
            }
            else
                throw new ValidacionExcepction("No se enviaron datos", "400");
        }

        public async Task ValidarIdAsync(int idDocente)
        {
            if (idDocente > 0)
            {
                if (!await _aspiranteRepository.ExistePorIdAsync(idDocente))
                    throw new ValidacionExcepction("No existe ese Aspirante.", "404");
            }
            else
                throw new ValidacionExcepction("La Id del Aspirante es inválida.", "400");
        }

        public async Task ValidarEdicionAsync(EditarDocenteDTO dto)
        {
            if (dto != null)
            {
                validarNombre(dto.Nombre);
                await ValidarIdAsync(dto.IdDocente);
                await validarGradosAsync(dto.GradosAgregados, dto.GradosEditados, dto.IdsGradosEliminados, dto.IdDocente);

                if (dto.NuevoArchivo)
                    validarArchivosGenerales(dto.ArchivosGenerales);
            }
            else
                throw new ValidacionExcepction("No se enviaron datos.", "400");
        }

        private void validarNombre(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                throw new ValidacionExcepction("El Nombre es obligatorio.", "400");
        }

        private void validarArchivosGenerales(CargarArchivoDTO dto)
        {
            if (string.IsNullOrEmpty(dto.NombreArchivo))
                throw new ValidacionExcepction("El Nombre del Archivo es obligatorio.", "400");
            if (string.IsNullOrEmpty(dto.RutaArchivo))
                throw new ValidacionExcepction("La Ruta del Archivo es obligatoria.", "400");
        }

        private async Task validarGradosAgregadosRegistro(List<AgregarGradoDTO> lista)
        {
            var hayUltimo = false;
            var cantidad = 0;

            if (lista.Count > 1)
            {
                foreach (var grado in lista)
                {

                    if (string.IsNullOrEmpty(grado.Grado))
                        throw new ValidacionExcepction("El Nombre del Grado es obligatorio.", "400");
                    if (!Constantes.GRADOS_DOCENTES.Contains(grado.Grado))
                        throw new ValidacionExcepction("El Nombre del Grado es inválido.", "400");
                    if (string.IsNullOrEmpty(grado.Titulo))
                        throw new ValidacionExcepction("El Titulo del Grado es obligatorio.", "400");
                    if (grado.Ultimo)
                    {
                        hayUltimo = true;
                        cantidad++;
                    }
                    await ValidarIdAsync(grado.IdDocente);
                }
            }
            else
            {
                throw new ValidacionExcepction("Debe haber al menos un Grado.", "400");
            }

            if (!hayUltimo || cantidad < 1)
                throw new ValidacionExcepction("Tiene que haber un Grado marcado como último.", "400");
            if (cantidad > 1)
                throw new ValidacionExcepction("Sólo puede hber un Grado marcado como último.", "400");
        }

        private async Task validarGradosAsync(List<AgregarGradoDTO> gradosAgregados, List<DatosGradoDTO> gradosEditados, List<int> gradosEliminados, int idDocente)
        {
            var grados = await _gradoRepository.ObtenerTodosAsync(idDocente);

            if (gradosEliminados.Count > 0)
            {
                foreach (var id in gradosEliminados)
                {
                    await validarIdGrado(id, idDocente);
                    var grado = grados.First(g => g.IdGrado == id);
                    grados.Remove(grado);
                }
            }

            if (gradosEditados.Count > 0)
            {
                foreach (var grado in gradosEditados)
                {
                    await validarIdGrado(grado.IdGrado, idDocente);
                    await ValidarIdAsync(grado.IdDocente);

                    if (grado.IdDocente != idDocente)
                        throw new ValidacionExcepction($"La Id del Aspirante del Grado editado con Id {grado.IdGrado} no concuerda con la Id del Aspirante editado.", "400");
                    if (string.IsNullOrEmpty(grado.Grado))
                        throw new ValidacionExcepction("El Nombre del Grado es obligatorio.", "400");
                    if (!Constantes.GRADOS_DOCENTES.Contains(grado.Grado))
                        throw new ValidacionExcepction("El Nombre del Grado es inválido.", "400");
                    if (string.IsNullOrEmpty(grado.Titulo))
                        throw new ValidacionExcepction("El Titulo del Grado es obligatorio.", "400");

                    var gradoExistente = grados.First(g => g.IdGrado == grado.IdGrado);
                    gradoExistente.Ultimo = grado.Ultimo;
                }
            }

            if (gradosAgregados.Count > 0)
            {
                foreach (var grado in gradosAgregados)
                {
                    await ValidarIdAsync(grado.IdDocente);

                    if (grado.IdDocente != idDocente)
                        throw new ValidacionExcepction("La Id del Aspirante de un Grado agregado no concuerda con la Id del Aspirante editado.", "400");
                    if (string.IsNullOrEmpty(grado.Grado))
                        throw new ValidacionExcepction("El Nombre del Grado es obligatorio.", "400");
                    if (!Constantes.GRADOS_DOCENTES.Contains(grado.Grado))
                        throw new ValidacionExcepction("El Nombre del Grado es inválido.", "400");
                    if (string.IsNullOrEmpty(grado.Titulo))
                        throw new ValidacionExcepction("El Titulo del Grado es obligatorio.", "400");
                }
            }

            var hayUltimoEnGrados = false;
            int cantidadUltimoEnGrados = 0;
            var hayUltimoEnNuevos = false;
            int cantidadUltimoEnNuevos = 0;

            foreach (var grado in grados)
            {
                if (grado.Ultimo)
                {
                    hayUltimoEnGrados = true;
                    cantidadUltimoEnGrados += 1;
                }
            }
            foreach (var grado in gradosAgregados)
            {
                if (grado.Ultimo)
                {
                    hayUltimoEnNuevos = true;
                    cantidadUltimoEnNuevos += 1;
                }
            }

            if (!hayUltimoEnGrados && !hayUltimoEnNuevos)
                throw new ValidacionExcepction("Tiene que haber un Grado marcado como último.", "400");
            if (hayUltimoEnGrados && hayUltimoEnNuevos)
                throw new ValidacionExcepction("Sólo puede hber un Grado marcado como último.", "400");
            if (cantidadUltimoEnGrados > 1 || cantidadUltimoEnNuevos > 1)
                throw new ValidacionExcepction("Sólo puede hber un Grado marcado como último.", "400");
        }

        private async Task validarIdGrado(int idGrado, int idDocente)
        {
            if (idGrado > 0)
            {
                var grado = await _gradoRepository.ObtenerAsync(idGrado);
                if (grado == null)
                    throw new ValidacionExcepction($"No existe ningún grado con la Id {idGrado}.", "404");
                else if (grado.IdDocente != idDocente)
                    throw new ValidacionExcepction($"La Id del Aspirante del Grado eliminado con Id {grado.IdGrado} no concuerda con la Id del Aspirante editado.", "400");
            }
            else
                throw new ValidacionExcepction("La Id del Grado es inválida.", "400");
        }
    }
}
