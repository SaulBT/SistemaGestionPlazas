using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class IntegranteCT
{
    public int idIntegranteCT { get; set; }

    public int idEntidadAcademica { get; set; }

    public string nombre { get; set; } = null!;

    public string cargo { get; set; } = null!;

    public virtual ICollection<Asistencia> Asistencia { get; set; } = new List<Asistencia>();

    public virtual EntidadAcademica idEntidadAcademicaNavigation { get; set; } = null!;
}
