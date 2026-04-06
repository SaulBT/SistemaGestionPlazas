using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class CoordinadorDGAA
{
    public int idCoordinadorDGAA { get; set; }

    public int idAreaAcademica { get; set; }

    public string nombre { get; set; } = null!;

    public string correo { get; set; } = null!;

    public string cargo { get; set; } = null!;

    public virtual AreaAcademica idAreaAcademicaNavigation { get; set; } = null!;
}
