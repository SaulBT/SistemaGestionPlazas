namespace SGPla.Models;

public partial class OfertaExcluidaAviso
{
    public int IdOfertaExcluidaAviso { get; set; }
    public int IdAviso { get; set; }
    public int IdOferta { get; set; }
    public string Motivo { get; set; } = null!;

    public virtual Aviso IdAvisoNavigation { get; set; } = null!;
    public virtual Oferta IdOfertaNavigation { get; set; } = null!;
}
