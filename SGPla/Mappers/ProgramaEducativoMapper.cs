using SGPla.Models;
using SGPla.Models.DTOs.ProgramaEducativo;

namespace SGPla.Mappers
{
    public class ProgramaEducativoMapper
    {
        public static ProgramaEducativo ToModel(CrearProgramaEducativoDTO dto)
        {
            return new ProgramaEducativo
            {
                Nombre = dto.Nombre,
                Campus = dto.Campus, 
                IdEntidadAcademica = dto.IdEntidadAcademica,
            };
        }

        public static ProgramaEducativo ToModel(EditarProgramaEducativoDTO dto)
        {
            return new ProgramaEducativo
            {
                IdProgramaEducativo = dto.IdProgramaEducativo,
                Nombre = dto.Nombre,
                Campus = dto.Campus,
                IdEntidadAcademica = dto.IdEntidadAcademica
            };

        }

        public static DetallesProgramaEducativoDTO ToDTO(ProgramaEducativo programaEducativo)
        {
            return new DetallesProgramaEducativoDTO
            {
                IdProgramaEducativo = programaEducativo.IdProgramaEducativo,
                Nombre = programaEducativo.Nombre,
                IdEntidadAcademica = programaEducativo.IdEntidadAcademica,

                IdAreaAcademica = programaEducativo.IdEntidadAcademicaNavigation?.IdAreaAcademica ?? 0,
                Region = programaEducativo.IdEntidadAcademicaNavigation?.Region,

                EntidadAcademica = programaEducativo.IdEntidadAcademicaNavigation?.Nombre,

                AreaAcademica = programaEducativo
                    .IdEntidadAcademicaNavigation?
                    .IdAreaAcademicaNavigation?
                    .Nombre,
                Campus = programaEducativo.Campus
            };
        }
    }
}
