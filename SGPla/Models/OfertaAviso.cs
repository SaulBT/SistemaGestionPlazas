using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class OfertaAviso
{
    public int idOfertaAviso { get; set; }

    public int idOferta { get; set; }

    public int idAviso { get; set; }

    public virtual Aviso idAvisoNavigation { get; set; } = null!;

    public virtual Oferta idOfertaNavigation { get; set; } = null!;
}
