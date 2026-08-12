namespace SGPla.Models.DTOs.Auth
{
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string Correo { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public string Rol { get; set; } = null!; // "Superusuario", "Entidad Academica", "Coordinador DGAA"
        public int? EntidadAcademicaId { get; set; }

        public bool EsSuperUsuario { get; set; }
    }
}
