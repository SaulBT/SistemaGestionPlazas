namespace SGPla.Models.Components
{
    public class ModalArchivoModel
    {
        public string Id { get; set; } = "modal";

        public string IdBandera { get; set; } = string.Empty;

        public string NombreArchivo { get; set; } = string.Empty;
        public string FormatosAceptados { get; set; } = string.Empty;
        public int TamanioMaximo { get; set; }
        public string OnConfirm { get; set; } = string.Empty;
        public string OnUpload { get; set; } = string.Empty;
        public string OnCancel { get; set; } = string.Empty;
    }
}
