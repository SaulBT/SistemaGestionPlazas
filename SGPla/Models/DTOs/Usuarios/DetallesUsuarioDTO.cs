namespace SGPla.Models.DTOs.Usuarios
{
    public class DetallesUsuarioDTO
    {
        public int IdUsuario { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;

        public int? IdAreaAcademica { get; set; }
        public string? NombreAreaAcademica { get; set; }

        public int? IdEntidadAcademica { get; set; }
        public string? NombreEntidadAcademica { get; set; }

        public string? Region { get; set; }
    }
}
