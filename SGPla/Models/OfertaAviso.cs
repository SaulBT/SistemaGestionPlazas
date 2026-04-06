using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class OfertaAviso
{
    public int IdOfertaAviso { get; set; }

    public int IdOferta { get; set; }

    public int IdAviso { get; set; }

    public virtual Aviso IdAvisoNavigation { get; set; } = null!;

    public virtual Ofertum IdOfertaNavigation { get; set; } = null!;
}
