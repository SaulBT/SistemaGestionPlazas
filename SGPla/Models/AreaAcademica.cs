using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class AreaAcademica
{
    public int IdAreaAcademica { get; set; }

    public string Nombre { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Extension { get; set; } = null!;

    public virtual ICollection<CoordinadorDgaa> CoordinadorDgaa { get; set; } = new List<CoordinadorDgaa>();

    public virtual ICollection<EntidadAcademica> EntidadAcademica { get; set; } = new List<EntidadAcademica>();
}
