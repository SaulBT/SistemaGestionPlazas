namespace SGPla.Models
{
    public class ModalModel
    {
        public string Id { get; set; } = "modal";

        public string Tipo { get; set; } = "info";
        // info | confirm

        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;

        public string TextoConfirmar { get; set; } = "Confirmar";
        public string TextoCancelar { get; set; } = "Cancelar";

        public string OnConfirm { get; set; } = string.Empty;
        public string OnCancel { get; set; } = string.Empty;

        public string? Body { get; set; }
        public string Accion { get; set; } = "Confirmar";
    }
}