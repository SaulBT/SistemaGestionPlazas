namespace SGPla.Models.Componentes
{
    public class ToastModel
    {
        public string Id { get; set; } = "toast";

        public string Titulo { get; set; } = "Notificación";

        public string Mensaje { get; set; } = "Operación realizada correctamente.";

        // success | error | warning | info
        public string Tipo { get; set; } = "success";

        public int Duracion { get; set; } = 5000;
    }
}