using SGPla.Models.DTOs.EntidadAcademica;

namespace SGPla.Validations.Interfaces
{
    public interface IEntidadAcademicaValidator
    {
        Task ValidarCreacionAsync(CrearEntidadAcademicaDTO crearEntidadAcademicaDTO);
        Task ValidarIdAsync(int Id);
        Task ValidarEdicionAsync(DatosEntidadAcademicaDTO datosEntidadAcademicaDTO);
        void ValidarIndice(int indice)
    }
}
