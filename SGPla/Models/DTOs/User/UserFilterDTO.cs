namespace SGPla.Models.DTOs.User
{
    public class UserFilterDTO
    {
        public string? Rol { get; set; }

        public string? Region { get; set; }
        public int? IdAreaAcademica { get; set; }
        public int? IdEntidadAcademica { get; set; }
    }
}
