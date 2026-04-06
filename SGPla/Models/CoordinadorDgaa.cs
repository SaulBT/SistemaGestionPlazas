using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class CoordinadorDgaa
{
    public int IdCoordinadorDgaa { get; set; }

    public int IdAreaAcademica { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Cargo { get; set; } = null!;

    public virtual AreaAcademica IdAreaAcademicaNavigation { get; set; } = null!;
}
