namespace SGPla.Models.DTOs.User
{
    public class UpdateUserDTO
    {
        public int IdUser { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;

        public int? IdAreaAcademica { get; set; }
        public int? IdEntidadAcademica { get; set; }
    }
}
