namespace SGPla.Models.DTOs.Grados
{
    public class AgregarGradoDTO
    {
        public int IdDocente { get; set; }
        public string Grado { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public bool Ultimo { get; set; } = false;
    }
}
