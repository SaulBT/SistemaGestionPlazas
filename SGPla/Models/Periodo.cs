using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Periodo
{
    public int IdPeriodo { get; set; }

    public string Codigo { get; set; } = null!;

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public virtual ICollection<Aviso> Aviso { get; set; } = new List<Aviso>();

    public virtual ICollection<CargaAcademica> CargaAcademica { get; set; } = new List<CargaAcademica>();

    public virtual ICollection<Oferta> Oferta { get; set; } = new List<Oferta>();
}
