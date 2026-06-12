using SGPla.Commons;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Docentes;
using SGPla.Models.DTOs.Grados;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPla.Validations.Implementations
{
    public class DocenteValidator : IDocenteValidator
    {
        private readonly IDocenteRepository _docenteRepository;
        private readonly IGradoRepository _gradoRepository;

        public DocenteValidator(IDocenteRepository docenteRepository, IGradoRepository gradoRepository)
        {
            _docenteRepository = docenteRepository;
            _gradoRepository = gradoRepository;
        }

        public async Task ValidarRegistroAsync(RegistrarDocenteDTO dto)
        {
            if (dto != null)
            {
                validarCamposDocente(dto.Nombre, dto.NumeroPersonal, dto.Puesto);
                if (await _docenteRepository.ExistePorNumeroAsync(dto.NumeroPersonal))
                    throw new ValidacionExcepction("Ya existe un Docente con ese número.", "409");
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
                if (!await _docenteRepository.ExistePorIdAsync(idDocente))
                    throw new ValidacionExcepction("No existe ese Docente.", "404");
            }
        }

        public async Task ValidarEdicionAsync(EditarDocenteDTO dto)
        {
            if (dto != null)
            {
                validarCamposDocente(dto.Nombre, dto.NumeroPersonal, dto.Puesto);
                await ValidarIdAsync(dto.IdDocente);
                await validarGradosAsync(dto.GradosAgregados, dto.GradosEditados, dto.IdsGradosEliminados, dto.IdDocente);
                
                if (dto.NuevoArchivo)
                    validarArchivosGenerales(dto.ArchivosGenerales);
            }
            else
                throw new ValidacionExcepction("No se enviaron datos.", "400");
        }

        private void validarCamposDocente(string nombre, string numeroPersonal, string puesto)
        {
            if (string.IsNullOrEmpty(nombre))
                throw new ValidacionExcepction("El Nombre es obligatorio.", "400");
            if (string.IsNullOrEmpty(numeroPersonal))
                throw new ValidacionExcepction("El Numero de Personal es obligatorio.", "400");
            if (!int.TryParse(numeroPersonal, out int numero) || numero <= 0)
                throw new ValidacionExcepction("El Numero de Personal debe de ser un entero mayor a 0.", "400");
            if (string.IsNullOrEmpty(puesto))
                throw new ValidacionExcepction("El Puesto es obligatorio.", "400");
            if (!Constantes.PUESTOS.Contains(puesto))
                throw new ValidacionExcepction("El Puesto no es válido.", "400");
        }

        private void validarArchivosGenerales(CargarArchivoDTO dto)
        {
            if (dto != null)
            {
                if (string.IsNullOrEmpty(dto.NombreArchivo))
                    throw new ValidacionExcepction("El Nombre del Archivo es obligatorio.", "400");
                if (string.IsNullOrEmpty(dto.RutaArchivo))
                    throw new ValidacionExcepction("La Ruta del Archivo es obligatoria.", "400");

            }
            else
                throw new ValidacionExcepction("No se cargó ningún archivo.", "400");
        }

        private async Task validarGradosAgregadosRegistro(List<AgregarGradoDTO> lista)
        {
            var hayUltimo = false;

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
                        hayUltimo = true;
                    await ValidarIdAsync(grado.IdDocente);
                }
            }
            else
            {
                throw new ValidacionExcepction("Debe haber al menos un Grado.", "400");
            }

            if (!hayUltimo)
                throw new ValidacionExcepction("Tiene que haber un Grado marcado como último.", "400");
        }

        private async Task validarGradosAsync(List<AgregarGradoDTO> gradosAgregados, List<DatosGradoDTO> gradosEditados, List<int> gradosEliminados, int idDocente)
        {
            var grados = await _gradoRepository.ObtenerTodosAsync(idDocente);
            var hayUltimo = false;

            if (gradosEliminados.Count > 0)
            {
                foreach (var id in gradosEliminados)
                {
                    await validarIdGrado(id);
                    var grado = grados.First(g => g.IdGrado == id);
                    grados.Remove(grado);
                }
            }

            if (gradosEditados.Count > 0)
            {
                foreach(var grado in gradosEditados)
                {
                    await validarIdGrado(grado.IdGrado);
                    await ValidarIdAsync(grado.IdDocente);

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
                foreach(var grado in gradosAgregados)
                {
                    await ValidarIdAsync(grado.IdDocente);

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

            foreach(var grado in grados)
            {
                if (grado.Ultimo)
                {
                    hayUltimoEnGrados = true;
                    cantidadUltimoEnGrados += 1;
                }
            }
            foreach(var grado in gradosAgregados)
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
                throw new ValidacionExcepction("No puede de haber más de un Grado marcado como último.", "400");
            if (cantidadUltimoEnGrados > 1 || cantidadUltimoEnNuevos > 1)
                throw new ValidacionExcepction("No puede de haber más de un Grado marcado como último.", "400");
        }

        private async Task validarIdGrado(int idGrado)
        {
            if (idGrado > 0)
            {
                if (!await _docenteRepository.ExistePorIdAsync(idGrado))
                    throw new ValidacionExcepction($"No existe ningún grado con la Id {idGrado}.", "404");
            }
            else
                throw new ValidacionExcepction("La Id del Grado es inválida.", "400");
        }
    }
}
