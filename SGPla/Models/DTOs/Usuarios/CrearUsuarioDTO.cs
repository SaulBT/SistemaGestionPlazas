namespace SGPla.Models.DTOs.Usuarios
{
    public class CrearUsuarioDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;

        public int? IdAreaAcademica { get; set; }
        public int? IdEntidadAcademica { get; set; }
    }
}
