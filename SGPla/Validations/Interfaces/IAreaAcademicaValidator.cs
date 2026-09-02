using SGPla.Models.DTOs.AreaAcademica;

namespace SGPla.Validations.Interfaces
{
    public interface IAreaAcademicaValidator
    {
        void ValidarCreacion(CrearAreaAcademicaDTO crearAreaAcademicaDTO);
        Task ValidarIdAsync(int id);
        Task ValidarEdicionAsync(DatosAreaAcademicaDTO datosAreaAcademicaDTO);
    }
}
