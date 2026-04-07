namespace SGPla.Models.DTOs.Usuario
{
    public class ListaUsuarioDTO
    {
        public int IdUser { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;

        public string? NombreAreaAcademica { get; set; }
        public string? NombreEntidadAcademica { get; set; }
        public string? Region { get; set; }
    }
}
