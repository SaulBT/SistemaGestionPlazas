public class ModalModel
{
    public string Id { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Mensaje { get; set; } = "";

    public string TextoConfirmar { get; set; } = "Confirmar";
    public string TextoCancelar { get; set; } = "Cancelar";

    public string ActionUrl { get; set; } = "";
}