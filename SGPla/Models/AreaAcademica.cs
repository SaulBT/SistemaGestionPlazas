using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class AreaAcademica
{
    public int idAreaAcademica { get; set; }

    public string nombre { get; set; } = null!;

    public string calleNumero { get; set; } = null!;

    public string colonia { get; set; } = null!;

    public string cp { get; set; } = null!;

    public string municipio { get; set; } = null!;

    public string telefono { get; set; } = null!;

    public string conmutador { get; set; } = null!;

    public string extension { get; set; } = null!;

    public string fax { get; set; } = null!;

    public virtual ICollection<CoordinadorDGAA> CoordinadorDGAA { get; set; } = new List<CoordinadorDGAA>();

    public virtual ICollection<EntidadAcademica> EntidadAcademica { get; set; } = new List<EntidadAcademica>();
}
