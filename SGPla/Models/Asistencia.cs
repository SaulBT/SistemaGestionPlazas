using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Asistencia
{
    public int IdAsistencia { get; set; }

    public int IdActa { get; set; }

    public int IdIntegranteCt { get; set; }

    public bool Asistio { get; set; }

    public virtual Acta IdActaNavigation { get; set; } = null!;

    public virtual IntegranteCt IdIntegranteCtNavigation { get; set; } = null!;
}
