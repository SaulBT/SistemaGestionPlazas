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
                Codigo = NormalizarCodigo(dto.Codigo),
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
                Codigo = NormalizarCodigo(dto.Codigo),
                Nombre = dto.Nombre,
                Campus = dto.Campus,
                IdEntidadAcademica = dto.IdEntidadAcademica
            };

        }

        public static DetallesProgramaEducativoDTO ToDTO(ProgramaEducativo programaEducativo)
        {
            var (codigo, nombre) = SepararCodigoYNombre(
                programaEducativo.Codigo,
                programaEducativo.Nombre);

            return new DetallesProgramaEducativoDTO
            {
                IdProgramaEducativo = programaEducativo.IdProgramaEducativo,
                Codigo = codigo,
                Nombre = nombre,
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

        private static (string Codigo, string Nombre) SepararCodigoYNombre(
            string? codigo,
            string nombre)
        {
            var nombreLimpio = nombre.Trim();
            var codigoLimpio = codigo?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(codigoLimpio)
                && nombreLimpio.StartsWith(codigoLimpio + "-", StringComparison.Ordinal))
            {
                return (codigoLimpio, nombreLimpio[(codigoLimpio.Length + 1)..].Trim());
            }

            if (string.IsNullOrWhiteSpace(codigoLimpio))
            {
                var separador = nombreLimpio.IndexOf('-');
                if (separador == 5)
                {
                    var posibleCodigo = nombreLimpio[..separador];
                    if (posibleCodigo.All(char.IsDigit))
                    {
                        return (posibleCodigo, nombreLimpio[(separador + 1)..].Trim());
                    }
                }
            }

            return (codigoLimpio, nombreLimpio);
        }

        private static string? NormalizarCodigo(string? codigo)
        {
            var codigoLimpio = codigo?.Trim();
            return string.IsNullOrWhiteSpace(codigoLimpio) ? null : codigoLimpio;
        }
    }
}
