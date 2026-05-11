namespace SGPla.Models.Components
{
    public class ModalFormularioModel
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
        public string PartialView { get; set; }
        public object? PartialModel { get; set; }

        public string OnConfirm { get; set; }
        public string OnCancel { get; set; }

        public string Accion { get; set; } = "Guardar";
    }
}
