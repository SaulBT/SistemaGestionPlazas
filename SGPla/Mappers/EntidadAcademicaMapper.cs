using SGPla.Models.DTOs.EntidadAcademica;

namespace SGPla.Mappers
{
    public class EntidadAcademicaMapper
    {

        public static OpcionEntidadAcademicaDTO ToOpcionDTO(Models.EntidadAcademica entidadAcademica)
        {
            return new OpcionEntidadAcademicaDTO
            {
                IdEntidadAcademica = entidadAcademica.IdEntidadAcademica,
                Nombre = entidadAcademica.Nombre
            };
        }
    }
}
