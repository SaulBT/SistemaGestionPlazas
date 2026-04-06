using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Grado
{
    public int IdGrado { get; set; }

    public int IdDocente { get; set; }

    public string Grado1 { get; set; } = null!;

    public string Titulo { get; set; } = null!;

    public bool Ultimo { get; set; }

    public virtual Docente IdDocenteNavigation { get; set; } = null!;
}
