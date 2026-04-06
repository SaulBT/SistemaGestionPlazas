using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Asistencia
{
    public int idAsistencia { get; set; }

    public int idActa { get; set; }

    public int idIntegranteCT { get; set; }

    public bool asistio { get; set; }

    public virtual Acta idActaNavigation { get; set; } = null!;

    public virtual IntegranteCT idIntegranteCTNavigation { get; set; } = null!;
}
