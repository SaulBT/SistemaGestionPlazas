using SGPla.Models.DTOs.AreaAcademica;
using SGPla.Models.DTOs.EntidadAcademica;

namespace SGPla.Mappers
{
    public class AreaAcademicaMapper
    {
        public static OpcionAreaAcademicaDTO ToOpcionDTO(Models.AreaAcademica entidadAcademica)
        {
            return new OpcionAreaAcademicaDTO
            {
                IdAreaAcademica = entidadAcademica.IdAreaAcademica,
                Nombre = entidadAcademica.Nombre
            };
        }
    }
}
