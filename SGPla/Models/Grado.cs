using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Grado
{
    public int idGrado { get; set; }

    public int idDocente { get; set; }

    public string grado { get; set; } = null!;

    public string titulo { get; set; } = null!;

    public bool ultimo { get; set; }

    public virtual Docente idDocenteNavigation { get; set; } = null!;
}
