using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class IntegranteCt
{
    public int IdIntegranteCt { get; set; }

    public int IdEntidadAcademica { get; set; }

    public string Nombre { get; set; } = null!;

    public string Cargo { get; set; } = null!;

    public virtual ICollection<Asistencia> Asistencia { get; set; } = new List<Asistencia>();

    public virtual EntidadAcademica IdEntidadAcademicaNavigation { get; set; } = null!;
}
