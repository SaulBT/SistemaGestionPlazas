using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Periodo
{
    public int IdPeriodo { get; set; }

    public string Nombre { get; set; } = null!;

    public int AnioInicio { get; set; }

    public virtual ICollection<Aviso> Aviso { get; set; } = new List<Aviso>();

    public virtual ICollection<Oferta> Oferta { get; set; } = new List<Oferta>();
}
