using SGPla.Models;
using SGPla.Models.DTOs.ProgramacionAcademica;

namespace SGPla.Mappers
{
    public class CargaAcademicaMapper
    {

        public static CargaAcademica ToModel(CargaConOfertaDTO dto)
        {
            return new CargaAcademica
            {
                IdDocente = dto.idDocente,
                IdPeriodo = dto.idPeriodo,
                IdExperienciaEducativa = dto.idExperienciaEducativa,
                Nrc = NormalizarOpcional(dto.Nrc),
                Plaza = NormalizarOpcional(dto.Plaza),
                TipoContratacion = ExtraerTipoContratacion(dto.TipoContratacion),
                HorasPago = dto.HorasPago,
                Imparte = dto.Imparte
            };
        }

        private static string? NormalizarOpcional(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static string? ExtraerTipoContratacion(string? tipoContratacion)
        {
            if (string.IsNullOrWhiteSpace(tipoContratacion))
                return null;

            return tipoContratacion.Split('-', 2)[0].Trim();
        }
    }
}