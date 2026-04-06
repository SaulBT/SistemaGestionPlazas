using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Log
{
    public int idLog { get; set; }

    public int idOferta { get; set; }

    public string mensaje { get; set; } = null!;

    public virtual Oferta idOfertaNavigation { get; set; } = null!;
}
