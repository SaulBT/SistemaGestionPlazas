using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class CoordinadorEA
{
    public int idCoordinadorEA { get; set; }

    public int idEntidadAcademica { get; set; }

    public string nombre { get; set; } = null!;

    public string correo { get; set; } = null!;

    public string cargo { get; set; } = null!;

    public virtual EntidadAcademica idEntidadAcademicaNavigation { get; set; } = null!;
}
