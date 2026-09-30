using SGPla.Models.DTOs.Oferta;

namespace SGPla.Models.DTOs.ProgramacionAcademica
{
    public class CrearProgramacionAcademicaDTO
    {
        public int IdRegion { get; set; } = 1;

        public int IdEntidadAcademica { get; set; } = 1;

        public int IdPeriodo { get; set; } = 1;

        public List<OfertaDTO> OfertasVacantes { get; set;  } = new List<OfertaDTO>();

        public List<OfertaDTO> OfertasAsignadas { get; set; } = new List<OfertaDTO>();
    }
}
