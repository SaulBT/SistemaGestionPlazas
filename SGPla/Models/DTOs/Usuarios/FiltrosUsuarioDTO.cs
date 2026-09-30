namespace SGPla.Models.DTOs.Usuarios
{
    public class FiltrosUsuarioDTO
    {
        public string? Rol { get; set; }

        public string? Region { get; set; }
        public int? IdAreaAcademica { get; set; }
        public int? IdEntidadAcademica { get; set; }

        public string? Busqueda { get; set; }

        //Paginación
        public int Pagina { get; set; } = 1;
        public int Cantidad { get; set; } = 10;
    }
}
