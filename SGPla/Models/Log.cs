using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Log
{
    public int IdLog { get; set; }

    public int IdOferta { get; set; }

    public string Mensaje { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public virtual Oferta IdOfertaNavigation { get; set; } = null!;
}
