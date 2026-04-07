using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class CoordinadorEa
{
    public int IdCoordinadorEa { get; set; }

    public int IdEntidadAcademica { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Cargo { get; set; } = null!;

    public virtual EntidadAcademica IdEntidadAcademicaNavigation { get; set; } = null!;
}
