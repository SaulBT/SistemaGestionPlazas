namespace SGPla.Models.DTOs.Auth
{
    public class ResultadoAutenticacion
    {
        public bool Exitoso { get; set; }
        public string? MensajeError { get; set; }
        public UsuarioDTO? Usuario { get; set; }
        public static ResultadoAutenticacion Fallido(string mensaje) =>
            new() { Exitoso = false, MensajeError = mensaje };

        public static ResultadoAutenticacion Ok(UsuarioDTO usuario) =>
            new() { Exitoso = true, Usuario = usuario };
    }
}
