namespace SGPla.Models.DTOs.Usuarios
{
    public class FiltrosUsuarioDTO
    {
        public string? Rol { get; set; }

        public string? Region { get; set; }
        public int? IdAreaAcademica { get; set; }
        public int? IdEntidadAcademica { get; set; }
    }
}
