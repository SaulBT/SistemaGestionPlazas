using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Periodo
{
    public int idPeriodo { get; set; }

    public string nombre { get; set; } = null!;

    public int anioInicio { get; set; }

    public virtual ICollection<Aviso> Aviso { get; set; } = new List<Aviso>();

    public virtual ICollection<Oferta> Oferta { get; set; } = new List<Oferta>();
}
