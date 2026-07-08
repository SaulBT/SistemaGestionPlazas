using SGPla.Models.DTOs.Oferta;

namespace SGPla.Models.DTOs.PlanEstudios
{
    public class OfertaPlanEstudiosAvisoDTO
    {
        public string Nombre { get; set; }
        public List<DatosOfertaAvisoDTO> Ofertas { get; set; }
    }
}
