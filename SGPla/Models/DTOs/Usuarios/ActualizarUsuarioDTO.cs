namespace SGPla.Models.DTOs.Usuario
{
    public class ActualizarUsuarioDTO
    {
        public int IdUser { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;

        public int? IdAreaAcademica { get; set; }
        public int? IdEntidadAcademica { get; set; }
    }
}
