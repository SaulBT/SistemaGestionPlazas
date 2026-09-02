using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Articulo
{
    public int IdArticulo { get; set; }

    public string Numero { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Aviso> Aviso { get; set; } = new List<Aviso>();

    public virtual ICollection<Oferta> Oferta { get; set; } = new List<Oferta>();
}
